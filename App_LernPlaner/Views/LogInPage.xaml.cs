using System.Windows;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views  
{
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnRecover_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Passwort wiederherstellen",
                Width = 1100,
                Height = 680,
                MinWidth = 900,
                MinHeight = 560,
                Background = System.Windows.Media.Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new RecoveryPage()
            };

            window.ShowDialog();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Bitte Benutzername und Passwort eingeben.",
                    "Ungültige Eingabe",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var result = await new AuthService().TryLoginAsync(username, password);

            if (!result.Success || result.User == null)
            {
                MessageBox.Show(
                    result.Error,
                    "Anmeldung fehlgeschlagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            sessionManager.CurrentUser = result.User;

            await new GamificationService().AwardDailyLoginAsync(result.User.UserId);

            Window dashboard = result.User.Role switch
            {
                Roles.Admin => new Admin.AdminDashboardWindow(),
                Roles.Lehrer => new Teacher.TeacherDashboardWindow(),
                _ => new Dashboard.DashboardWindow()
            };

            Application.Current.MainWindow = dashboard;
            dashboard.Show();

            Close();
        }
    }
}
