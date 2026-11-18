using System.Windows;
using System.Windows.Threading;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Teacher
{
    public partial class TeacherDashboardWindow : Window
    {
        private readonly NotificationService _notifications = new();
        private readonly DispatcherTimer _pollTimer;

        public TeacherDashboardWindow()
        {
            InitializeComponent();

            AppNavigator.NavigateAction = page => MainFrame.Navigate(page);

            var user = sessionManager.CurrentUser;
            if (user != null)
            {
                LblInitials.Text = user.Initials;
                LblName.Text = user.FullName;
            }

            MainFrame.Navigate(new Classes.KlassenPage());

            _pollTimer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(30) };
            _pollTimer.Tick += async (_, _) => await RefreshNotifBadgeAsync();
            _pollTimer.Start();
            Loaded += async (_, _) => await RefreshNotifBadgeAsync();
            Closed += (_, _) => _pollTimer.Stop();
        }

        private async System.Threading.Tasks.Task RefreshNotifBadgeAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            int unread = await _notifications.CountUnreadAsync(user.UserId);
            if (unread > 0)
            {
                LblNotifBadge.Text = unread.ToString();
                NotifBadge.Visibility = Visibility.Visible;
            }
            else
            {
                NotifBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnClasses_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Classes.KlassenPage());
        private void BtnTasks_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new TaskEditorListPage());
        private void BtnGrading_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SubmissionReviewTaskListPage());
        private void BtnRanking_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new RankingPage());
        private void BtnStatistik_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Statistics.StatistikPage());
        private void BtnKalender_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new KalenderPage());
        private void BtnNotifications_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new NotificationsPage());
        private void BtnChat_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Chat.ChatPage());
        private void BtnProfil_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            await sessionManager.LogoutAsync();
            var login = new LoginPage();
            Application.Current.MainWindow = login;
            login.Show();
            Close();
        }
    }
}
