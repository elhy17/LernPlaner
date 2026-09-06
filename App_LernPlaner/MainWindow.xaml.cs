using System.Windows;
using App_LernPlaner.Models;
using App_LernPlaner.Services;
using App_LernPlaner.Views;

namespace Nexus_Team
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowLoginPage();
        }

        private void ShowLoginPage()
        {
            MainFrame.Navigate(new LoginPage());
        }

        /// <summary>
        /// Wird nach erfolgreichem Login aufgerufen (siehe LogInPage.xaml.cs). Öffnet je nach
        /// Rolle das passende Dashboard-Fenster und schließt dieses Login-Fenster.
        /// </summary>
        public void GoToDashboard(User user)
        {
            Window dashboard = user.Role switch
            {
                Roles.Admin => new App_LernPlaner.Views.Admin.AdminDashboardWindow(),
                Roles.Lehrer => new App_LernPlaner.Views.Teacher.TeacherDashboardWindow(),
                _ => new App_LernPlaner.Views.Dashboard.DashboardWindow()
            };

            Application.Current.MainWindow = dashboard;
            dashboard.Show();
            Close();
        }
    }
}
