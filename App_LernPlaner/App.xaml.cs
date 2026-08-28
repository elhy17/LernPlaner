using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

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

            // Viele Views rufen die Datenbank über async void Event-Handler auf. Ohne diesen
            // Handler würde ein DB-/Netzwerkfehler (z.B. Verbindungsabbruch zu mysql.pb.bib.de)
            // die ganze App abstürzen lassen, statt nur eine Fehlermeldung zu zeigen.
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            try
            {
                var window = new Nexus_Team.MainWindow();
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

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("Unhandled exception: " + e.Exception);
            MessageBox.Show(
                "Es ist ein unerwarteter Fehler aufgetreten:\n" + e.Exception.Message +
                "\n\nBitte prüfe die Internet-/Datenbankverbindung und versuche es erneut.",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }

}
