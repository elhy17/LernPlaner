using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; 
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using App_LernPlaner.Models;

namespace Nexus_Team.views
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
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

            if (chkTerms.IsChecked != true)
            {
                MessageBox.Show("Bitte akzeptieren Sie die Nutzungsbedingungen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string username = txtRegUser.Text.Trim();
            string role = "Schüler";
            if (cbRole.SelectedItem is ComboBoxItem selectedRoleItem)
            {
                role = selectedRoleItem.Content.ToString();
            }

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var regAttempt = new addUser(
                    username,
                    hashedPassword,
                    email,
                    role,
                    "default.png",
                    0
                );

                if (regAttempt.IsSuccessful)
                {
                    MessageBox.Show(regAttempt.Message, "Erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (this.NavigationService != null)
                    {
                        this.NavigationService.GoBack();
                    }
                }
                else
                {
                    MessageBox.Show(regAttempt.Message, "Registrierung fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService?.Navigate(new LoginPage());
            }
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