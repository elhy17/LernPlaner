using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace Nexus_Team.views
{
    /// <summary>
    /// Konto anlegen - nur erreichbar über das Admin-Dashboard (Benutzerverwaltung).
    /// Es gibt keinen öffentlichen Registrierungsweg mehr (FA-01 ist admin-only).
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            lblEmailError.Visibility = Visibility.Collapsed;
            lblPassError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(txtRegUser.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPass.Password))
            {
                MessageBox.Show("Bitte füllen Sie alle Pflichtfelder aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string email = txtEmail.Text.Trim();
            if (!IsValidEmail(email))
            {
                lblEmailError.Visibility = Visibility.Visible;
                return;
            }

            string password = txtPass.Password;
            if (!IsPasswordStrong(password))
            {
                lblPassError.Visibility = Visibility.Visible;
                return;
            }

            string username = txtRegUser.Text.Trim();
            string role = Roles.Schueler;
            if (cbRole.SelectedItem is ComboBoxItem selectedRoleItem)
            {
                role = selectedRoleItem.Content.ToString() switch
                {
                    "Lehrer" => Roles.Lehrer,
                    _ => Roles.Schueler
                };
            }

            var result = await new AuthService().TryRegisterAsync(username, password, email, role);

            if (result.Success)
            {
                MessageBox.Show($"Konto für \"{username}\" ({role}) wurde angelegt.", "Erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                GoBackToAdmin();
            }
            else
            {
                MessageBox.Show(result.Error, "Konto anlegen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => GoBackToAdmin();

        private void GoBackToAdmin()
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                AppNavigator.NavigateTo(new App_LernPlaner.Views.Admin.AdminPage());
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        private bool IsPasswordStrong(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsDigit)) return false;
            return true;
        }
    }
}
