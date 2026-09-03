using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Services;
using App_LernPlaner.ViewModels;

namespace App_LernPlaner.Views
{
    // FA-06: eigenes Passwort ändern (für Schüler, Lehrer und Admin gleichermaßen,
    // da diese UserControl von ProfilePage für alle Rollen verwendet wird).
    public partial class ChangePasswortView : UserControl
    {
        public ChangePasswortView()
        {
            InitializeComponent();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            string current = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Bitte alle Felder ausfüllen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirm)
            {
                MessageBox.Show("Die Passwörter stimmen nicht überein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnSave.IsEnabled = false;
            try
            {
                var (success, error) = await new AuthService().TryChangePasswordAsync(user.UserId, current, newPassword);

                if (!success)
                {
                    MessageBox.Show(error, "Passwort ändern fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CurrentPasswordBox.Password = string.Empty;
                NewPasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;

                MessageBox.Show("Passwort wurde geändert.", "Erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);

                if (DataContext is ProfileViewModel vm)
                    vm.OnPasswordChanged();
            }
            finally
            {
                BtnSave.IsEnabled = true;
            }
        }
    }
}
