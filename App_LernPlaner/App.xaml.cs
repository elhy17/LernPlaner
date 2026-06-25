using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace App_LernPlaner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var window = new Views.Dashboard.DashboardWindow();
                window.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Startup exception: " + ex);
                MessageBox.Show("Fehler beim Starten der Anwendung:\n" + ex.Message,
                                "Startfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }
    }

}
