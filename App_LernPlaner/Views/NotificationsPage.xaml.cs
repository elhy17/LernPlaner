using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views
{
    // FA-25: Benachrichtigungen im Dashboard anzeigen und markieren
    public partial class NotificationsPage : Page
    {
        private readonly NotificationService _notificationService = new();
        private readonly DispatcherTimer _pollTimer;

        public NotificationsPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();

            // Live-Aktualisierung alle 30 Sekunden, solange die Seite offen ist.
            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _pollTimer.Tick += async (_, _) => await LoadAsync();
            _pollTimer.Start();
            Unloaded += (_, _) => _pollTimer.Stop();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            NotifList.ItemsSource = await _notificationService.GetForUserAsync(user.UserId);
        }

        // Einzelne Benachrichtigung als gelesen markieren, indem man sie anklickt
        // (statt alles automatisch beim Öffnen der Seite zu markieren).
        private async void NotifList_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (NotifList.SelectedItem is not Notification notif || notif.IsRead) return;

            await _notificationService.MarkAsReadAsync(notif.NotifId);
            await LoadAsync();
        }

        private async void BtnMarkAll_Click(object sender, RoutedEventArgs e)
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            await _notificationService.MarkAllAsReadAsync(user.UserId);
            await LoadAsync();
        }
    }
}
