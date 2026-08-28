using System.Windows;
using System.Windows.Threading;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Admin
{
    public partial class AdminDashboardWindow : Window
    {
        private readonly MessageService _messages = new();
        private readonly DispatcherTimer _pollTimer;

        public AdminDashboardWindow()
        {
            InitializeComponent();

            AppNavigator.NavigateAction = page => MainFrame.Navigate(page);

            var user = sessionManager.CurrentUser;
            if (user != null)
            {
                LblInitials.Text = user.Initials;
                LblName.Text = user.FullName;
            }

            MainFrame.Navigate(new AdminPage());

            _pollTimer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(30) };
            _pollTimer.Tick += async (_, _) => await RefreshRequestsBadgeAsync();
            _pollTimer.Start();
            Loaded += async (_, _) => await RefreshRequestsBadgeAsync();
            Closed += (_, _) => _pollTimer.Stop();
        }

        private async System.Threading.Tasks.Task RefreshRequestsBadgeAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            int unread = await _messages.CountUnreadAsync(user.UserId);
            if (unread > 0)
            {
                LblRequestsBadge.Text = unread.ToString();
                RequestsBadge.Visibility = Visibility.Visible;
            }
            else
            {
                RequestsBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminPage());
        private void BtnClasses_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminClassesPage());
        private void BtnRequests_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminRequestsPage());
        private void BtnStatistik_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Statistics.StatistikPage());
        private void BtnNotifications_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new NotificationsPage());
        private void BtnChat_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Chat.ChatPage());
        private void BtnProfil_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            sessionManager.Logout();
            var login = new Nexus_Team.MainWindow();
            Application.Current.MainWindow = login;
            login.Show();
            Close();
        }
    }
}
