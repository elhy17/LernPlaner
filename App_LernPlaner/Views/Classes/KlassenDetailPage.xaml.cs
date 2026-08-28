using System.Linq;
using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Classes
{
    public class AddStudentOption
    {
        public int UserId { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    // FA-09 (Schüler hinzufügen) / FA-10 (Schüler entfernen) / FA-11 (Klasse archivieren)
    public partial class KlassenDetailPage : Page
    {
        private readonly Class _cls;
        private readonly ClassService _classService = new();
        private readonly StatisticsService _statisticsService = new();
        private readonly Repositories.UserRepository _users = new();

        public KlassenDetailPage(Class cls)
        {
            InitializeComponent();
            _cls = cls;

            LblClassName.Text = cls.Name;
            LblMeta.Text = $"Einladungscode: {cls.JoinCode}";

            var user = sessionManager.CurrentUser;
            bool canManage = user != null && (user.IsAdmin || user.UserId == cls.TeacherId);
            BtnDeleteClass.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
            PanelAddStudent.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;

            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var stats = await _statisticsService.GetClassStatisticsAsync(_cls);
            LblStats.Text = $"{stats.StudentCount} Schüler · {stats.TaskCount} Aufgaben · {stats.SubmissionCount} Abgaben · {stats.SubmissionRate}% Abgabequote";

            var members = await _classService.GetMembersAsync(_cls.ClassId, role: 2);
            StudentsList.ItemsSource = members;

            if (PanelAddStudent.Visibility == Visibility.Visible)
            {
                var memberIds = members.Select(m => m.UserId).ToHashSet();
                var allUsers = await _users.GetAllAsync();
                CmbAddStudent.ItemsSource = allUsers
                    .Where(u => u.IsStudent && !memberIds.Contains(u.UserId))
                    .Select(u => new AddStudentOption { UserId = u.UserId, DisplayText = $"{u.UserName} ({u.FullName})" })
                    .ToList();
            }
        }

        private async void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (CmbAddStudent.SelectedItem is not AddStudentOption selected) return;

            var (success, error) = await _classService.AddStudentAsync(_cls.ClassId, selected.UserId);
            if (!success)
            {
                MessageBox.Show(error, "Hinzufügen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await LoadAsync();
        }

        private async void BtnRemoveStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int studentId) return;

            var result = MessageBox.Show("Diesen Schüler wirklich aus der Klasse entfernen?", "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            await _classService.RemoveMemberAsync(_cls.ClassId, studentId);
            await LoadAsync();
        }

        private async void BtnDeleteClass_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Klasse \"{_cls.Name}\" wirklich archivieren? Zugehörige Aufgaben bleiben erhalten, verlieren aber die Klassenzuordnung.",
                "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            await _classService.DeleteClassAsync(_cls.ClassId);
            AppNavigator.NavigateTo(new KlassenPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => AppNavigator.NavigateTo(new KlassenPage());
    }
}
