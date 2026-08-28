using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
}
