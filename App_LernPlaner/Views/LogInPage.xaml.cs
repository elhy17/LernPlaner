using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Services;

namespace Nexus_Team.views
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnRecover_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RecoveryPage());
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Bitte Benutzername und Passwort eingeben.", "Ungültige Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await new AuthService().TryLoginAsync(username, password);

            if (!result.Success || result.User == null)
            {
                MessageBox.Show(result.Error, "Anmeldung fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            sessionManager.CurrentUser = result.User;
            await new GamificationService().AwardDailyLoginAsync(result.User.UserId);

            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.GoToDashboard(result.User);
            }
        }
    }
}
