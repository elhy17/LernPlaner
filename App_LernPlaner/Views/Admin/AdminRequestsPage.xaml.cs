using System.Linq;
using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Admin
{
    // "Anfragen": der Admin sieht hier seine gesamte Chat-Inbox gebündelt (nur auf seiner Seite sichtbar).
    public partial class AdminRequestsPage : Page
    {
        private readonly MessageService _messages = new();

        public AdminRequestsPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            RequestsList.ItemsSource = await _messages.GetInboxAsync(user.UserId);
        }

        private async void BtnReply_Click(object sender, RoutedEventArgs e)
        {
            var admin = sessionManager.CurrentUser;
            if (admin == null || sender is not Button btn || btn.Tag is not int receiverId) return;

            if (btn.Parent is not StackPanel row) return;
            var textBox = row.Children.OfType<TextBox>().FirstOrDefault();
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text)) return;

            await _messages.SendAsync(admin.UserId, receiverId, textBox.Text.Trim());
            textBox.Text = string.Empty;
            MessageBox.Show("Antwort gesendet.", "Erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
