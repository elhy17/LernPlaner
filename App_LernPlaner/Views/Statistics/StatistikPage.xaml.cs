using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Statistics
{
    // FA-27 (Schüler), FA-28 (Lehrer), FA-29 (Admin)
    public partial class StatistikPage : Page
    {
        private readonly StatisticsService _statisticsService = new();
        private readonly ClassService _classService = new();
        private readonly StudyService _studyService = new();
        private readonly SubmissionService _submissionService = new();
        private readonly TaskService _taskService = new();
        private readonly Repositories.UserRepository _users = new();
        private List<Class> _teacherClasses = new();

        public StatistikPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            if (user.IsStudent)
            {
                PanelStudent.Visibility = Visibility.Visible;
                PanelTeacher.Visibility = Visibility.Collapsed;

                var stats = await _statisticsService.GetStudentStatsAsync(user);
                LblOpen.Text = stats.OffeneAufgaben.ToString();
                LblSubmitted.Text = stats.Abgegeben.ToString();
                LblGrade.Text = stats.Durchschnittsnote;

                int minutes = await _statisticsService.GetTotalStudyMinutesAsync(user.UserId);
                LblStudyTime.Text = minutes >= 60 ? $"{minutes / 60}h {minutes % 60}m" : $"{minutes}m";

                await LoadSubjectStatsAsync(user);
            }
            else
            {
                PanelStudent.Visibility = Visibility.Collapsed;
                PanelTeacher.Visibility = Visibility.Visible;

                var allClasses = await _classService.GetAllClassesAsync();
                _teacherClasses = user.IsAdmin ? allClasses : allClasses.Where(c => c.TeacherId == user.UserId).ToList();

                var rows = new List<ClassStatistics>();
                foreach (var c in _teacherClasses)
                    rows.Add(await _statisticsService.GetClassStatisticsAsync(c));

                ClassStatsList.ItemsSource = rows;

                if (user.IsAdmin)
                {
                    PanelAdmin.Visibility = Visibility.Visible;
                    var sys = await _statisticsService.GetSystemStatisticsAsync();
                    LblSystemStats.Text =
                        $"Benutzer gesamt: {sys.TotalUsers} (Schüler: {sys.TotalStudents}, Lehrer: {sys.TotalTeachers})\n" +
                        $"Klassen: {sys.TotalClasses}\nAufgaben: {sys.TotalTasks}\nAbgaben: {sys.TotalSubmissions}";
                }
            }
        }

        private async System.Threading.Tasks.Task LoadSubjectStatsAsync(User user)
        {
            var tasks = await _taskService.GetVisibleTasksForUserAsync(user);

            var rows = tasks
                .Where(task => !string.IsNullOrWhiteSpace(task.Subject))
                .GroupBy(task => task.Subject.Trim())
                .Select((group, index) =>
                {
                    int total = group.Count();
                    int completed = group.Count(task => task.Status >= 2);
                    int percentage = total == 0
                        ? 0
                        : (int)System.Math.Round(completed * 100.0 / total);

                    return new SubjectStatisticsRow
                    {
                        Subject = group.Key,
                        TotalTasks = total,
                        CompletedTasks = completed,
                        Percentage = percentage,
                        PercentageText = $"{percentage}%",
                        DetailText = $"{completed} von {total} erledigt",
                        BarBrush = GetSubjectBrush(index)
                    };
                })
                .OrderByDescending(row => row.Percentage)
                .ThenBy(row => row.Subject)
                .ToList();

            SubjectStatsList.ItemsSource = rows;
            SubjectStatsEmptyText.Visibility = rows.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private static Brush GetSubjectBrush(int index)
        {
            string[] colors =
            {
                "#2563EB",
                "#0EA5E9",
                "#16A34A",
                "#F97316",
                "#8B5CF6",
                "#EAB308"
            };

            return (Brush)new BrushConverter().ConvertFromString(colors[index % colors.Length])!;
        }

        private async void BtnLogSession_Click(object sender, RoutedEventArgs e)
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            if (!int.TryParse(TxtMinutes.Text.Trim(), out int minutes) || minutes <= 0)
            {
                MessageBox.Show("Bitte eine gültige Anzahl an Minuten eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnLogSession.IsEnabled = false;
            try
            {
                await _studyService.LogSessionAsync(user.UserId, 0, minutes, TxtNotes.Text.Trim());
                TxtMinutes.Text = "";
                TxtNotes.Text = "";
                await LoadAsync();
            }
            finally
            {
                BtnLogSession.IsEnabled = true;
            }
        }

        // FA-34: Noten und Statistiken exportieren
        private async void BtnExportGrades_Click(object sender, RoutedEventArgs e)
        {
            if (_teacherClasses.Count == 0)
            {
                MessageBox.Show("Keine Klassen zum Exportieren vorhanden.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Noten exportieren",
                Filter = "CSV-Datei (*.csv)|*.csv",
                FileName = "Noten.csv"
            };
            if (dialog.ShowDialog() != true) return;

            var users = await _users.GetAllAsync();
            var exporter = new ExportService();
            bool wroteHeader = false;

            foreach (var c in _teacherClasses)
            {
                var classTasks = (await _taskService.GetTasksByClassIdAsync(c.ClassId)).ToList();
                foreach (var t in classTasks)
                {
                    var submissions = await _submissionService.GetSubmissionsForTaskAsync(t.TaskId);
                    var rows = submissions.Where(s => s.IsGraded).Select(s => (
                        StudentName: users.FirstOrDefault(u => u.UserId == s.UserId)?.UserName ?? $"User {s.UserId}",
                        Grade: s.Grade ?? "",
                        Feedback: s.Feedback ?? "",
                        SubmittedAt: s.SubmittedAt));

                    if (!rows.Any()) continue;

                    exporter.AppendGradesToCsv(dialog.FileName, t.Titel, rows, includeHeader: !wroteHeader);
                    wroteHeader = true;
                }
            }

            MessageBox.Show("Noten wurden exportiert.", "Erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class SubjectStatisticsRow
    {
        public string Subject { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int Percentage { get; set; }
        public string PercentageText { get; set; } = "0%";
        public string DetailText { get; set; } = string.Empty;
        public Brush BarBrush { get; set; } = Brushes.DodgerBlue;
    }
}
