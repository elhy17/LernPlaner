using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views
{
    // FA-25: Benachrichtigungen im Dashboard anzeigen und markieren
    public partial class NotificationsPage : Page
    {
        private readonly NotificationService _notificationService = new();

        public NotificationsPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var items = await _notificationService.GetForUserAsync(user.UserId);
            NotifList.ItemsSource = items;

            foreach (var n in items)
                await _notificationService.MarkAsReadAsync(n.NotifId);
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
