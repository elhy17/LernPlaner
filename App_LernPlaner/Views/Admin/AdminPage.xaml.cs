using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using App_LernPlaner.Data;
using App_LernPlaner.Models;
using App_LernPlaner.Repositories;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Admin
{
    public class AdminUserRowVm
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string ActiveButtonText => IsActive ? "Deaktivieren" : "Aktivieren";
        public Brush ActiveButtonBrush => IsActive ? new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)) : new SolidColorBrush(Color.FromRgb(0x7F, 0x1D, 0x1D));
    }

    // Admin-Bereich: Benutzerverwaltung (FA-05, 3.2.1) und systemweite Statistiken (FA-29)
    public partial class AdminPage : Page
    {
        private readonly UserRepository _users = new();
        private readonly StatisticsService _statisticsService = new();
        private List<AdminUserRowVm> _allUsers = new();

        public AdminPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var sys = await _statisticsService.GetSystemStatisticsAsync();
            LblSystemStats.Text = $"Benutzer: {sys.TotalUsers} · Schüler: {sys.TotalStudents} · Lehrer: {sys.TotalTeachers} · " +
                                   $"Klassen: {sys.TotalClasses} · Aufgaben: {sys.TotalTasks} · Abgaben: {sys.TotalSubmissions}";

            var users = AppConfig.UseMockData ? MockDatabase.Users : await _users.GetAllAsync();

            _allUsers = users.Select(u => new AdminUserRowVm
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive
            }).ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string term = (TxtSearch.Text ?? string.Empty).Trim();
            UsersList.ItemsSource = string.IsNullOrEmpty(term)
                ? _allUsers
                : _allUsers.Where(u =>
                    u.UserName.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(term, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void BtnNewAccount_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.NavigateTo(new Nexus_Team.views.RegisterPage());
        }

        private async void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int userId) return;

            if (userId == sessionManager.CurrentUser?.UserId)
            {
                MessageBox.Show("Du kannst deine eigene Rolle nicht ändern.", "Nicht erlaubt", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User? user = AppConfig.UseMockData ? MockDatabase.Users.FirstOrDefault(u => u.UserId == userId) : await _users.GetByIdAsync(userId);
            if (user == null) return;

            string newRole = user.Role switch
            {
                Roles.Schueler => Roles.Lehrer,
                Roles.Lehrer => Roles.Admin,
                _ => Roles.Schueler
            };

            if (AppConfig.UseMockData) user.Role = newRole;
            else await _users.UpdateRoleAsync(userId, newRole);

            await LoadAsync();
        }

        private async void ToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int userId) return;

            if (userId == sessionManager.CurrentUser?.UserId)
            {
                MessageBox.Show("Du kannst dein eigenes Konto nicht deaktivieren.", "Nicht erlaubt", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User? user = AppConfig.UseMockData ? MockDatabase.Users.FirstOrDefault(u => u.UserId == userId) : await _users.GetByIdAsync(userId);
            if (user == null) return;

            bool newActive = !user.IsActive;

            if (AppConfig.UseMockData) user.IsActive = newActive;
            else await _users.UpdateActiveAsync(userId, newActive);

            await LoadAsync();
        }
    }
}
