using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Services;
using App_LernPlaner.Data;

namespace App_LernPlaner.Views.Classes
{
    public class ClassRowVm
    {
        public int ClassId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int TaskCount { get; set; }
        public Visibility ShowJoinCode { get; set; }
    }

    // FA-07 bis FA-11: Klassen-System
    public partial class KlassenPage : Page
    {
        private readonly ClassService _classService = new();

        public KlassenPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadClassesAsync();
        }

        private async System.Threading.Tasks.Task LoadClassesAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var classes = user.IsTeacher || user.IsAdmin
                ? (user.IsAdmin ? await _classService.GetAllClassesAsync() : (await _classService.GetAllClassesAsync()).Where(c => c.TeacherId == user.UserId).ToList())
                : await _classService.GetClassesForUserAsync(user.UserId);

            var rows = new List<ClassRowVm>();
            foreach (var c in classes)
            {
                var members = await _classService.GetMembersAsync(c.ClassId);
                var teacher = members.FirstOrDefault(m => m.UserId == c.TeacherId);
                var tasks = AppConfig.UseMockData
                    ? MockDatabase.Tasks.Count(t => t.ClassId == c.ClassId)
                    : (await new TaskService().GetTasksAsync()).Count(t => t.ClassId == c.ClassId);

                rows.Add(new ClassRowVm
                {
                    ClassId = c.ClassId,
                    Name = c.Name,
                    TeacherName = teacher?.UserName ?? "-",
                    JoinCode = c.JoinCode,
                    StudentCount = await _classService.CountStudentsAsync(c.ClassId),
                    TaskCount = tasks,
                    ShowJoinCode = user.IsTeacher || user.IsAdmin ? Visibility.Visible : Visibility.Collapsed
                });
            }

            ClassesList.ItemsSource = rows;

            bool canCreate = user.IsTeacher || user.IsAdmin;
            BtnCreateClass.Visibility = canCreate ? Visibility.Visible : Visibility.Collapsed;
            BtnJoinClass.Visibility = canCreate ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnCreateClass_Click(object sender, RoutedEventArgs e)
        {
            PanelJoin.Visibility = Visibility.Collapsed;
            PanelCreate.Visibility = Visibility.Visible;
            TxtNewClassName.Focus();
        }

        private void BtnCancelCreate_Click(object sender, RoutedEventArgs e) => PanelCreate.Visibility = Visibility.Collapsed;

        private async void BtnConfirmCreate_Click(object sender, RoutedEventArgs e)
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            string name = TxtNewClassName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowFormError("Bitte einen Klassennamen eingeben.");
                return;
            }

            var created = await _classService.CreateClassAsync(name, user.UserId);
            TxtNewClassName.Text = "";
            PanelCreate.Visibility = Visibility.Collapsed;
            LblFormError.Visibility = Visibility.Collapsed;

            MessageBox.Show($"Klasse \"{created.Name}\" erstellt. Einladungscode: {created.JoinCode}", "Erfolgreich");
            await LoadClassesAsync();
        }

        private void BtnJoinClass_Click(object sender, RoutedEventArgs e)
        {
            PanelCreate.Visibility = Visibility.Collapsed;
            PanelJoin.Visibility = Visibility.Visible;
            TxtJoinCode.Focus();
        }

        private void BtnCancelJoin_Click(object sender, RoutedEventArgs e) => PanelJoin.Visibility = Visibility.Collapsed;

        private async void BtnConfirmJoin_Click(object sender, RoutedEventArgs e)
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            string code = TxtJoinCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                ShowFormError("Bitte einen Einladungscode eingeben.");
                return;
            }

            var (success, error) = await _classService.TryJoinClassAsync(code, user.UserId);
            if (success)
            {
                TxtJoinCode.Text = "";
                PanelJoin.Visibility = Visibility.Collapsed;
                LblFormError.Visibility = Visibility.Collapsed;
                await LoadClassesAsync();
            }
            else
            {
                ShowFormError(error);
            }
        }

        private void ShowFormError(string message)
        {
            LblFormError.Text = message;
            LblFormError.Visibility = Visibility.Visible;
        }

        private async void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int classId)
            {
                var classes = await _classService.GetAllClassesAsync();
                var cls = classes.First(c => c.ClassId == classId);
                AppNavigator.NavigateTo(new KlassenDetailPage(cls));
            }
        }
    }
}
