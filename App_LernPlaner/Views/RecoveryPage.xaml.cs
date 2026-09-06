using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views
{
    /// <summary>
    /// Interaction logic for RecoveryPage.xaml
    /// </summary>
    public partial class RecoveryPage : Page
    {
        public RecoveryPage()
        {
            InitializeComponent();
        }

        // FA-04: Passwort zurücksetzen. Es ist kein SMTP konfiguriert (siehe EmailService),
        // daher wird das neue temporäre Passwort direkt angezeigt statt per E-Mail versendet.
        private async void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Bitte E-Mail-Adresse eingeben.", "Ungültige Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (success, _, tempPasswordOrError) = await new AuthService().TryResetPasswordAsync(email);

            if (!success)
            {
                MessageBox.Show(tempPasswordOrError, "Zurücksetzen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(
                $"Dein neues temporäres Passwort lautet:\n\n{tempPasswordOrError}\n\nBitte melde dich damit an und ändere es anschließend in deinem Profil.",
                "Passwort zurückgesetzt", MessageBoxButton.OK, MessageBoxImage.Information);

            Window.GetWindow(this)?.Close();
        }

        private void BtnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
