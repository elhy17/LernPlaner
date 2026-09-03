using App_LernPlaner.Commands;
using App_LernPlaner.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace App_LernPlaner.ViewModels
{
    public class DashboardWindowViewModel : INotifyPropertyChanged
    {
        private readonly NotificationService _notificationService = new();
        private readonly DispatcherTimer _pollTimer;

        private Page? _currentPage;

        public Page? CurrentPage
        {
            get => _currentPage;
            set
            {
               _currentPage = value;
              OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public string Initials { get; }
        public string DisplayName { get; }
        public string RoleLevelText { get; }

        private int _unreadNotifications;
        public int UnreadNotifications
        {
            get => _unreadNotifications;
            set { _unreadNotifications = value; OnPropertyChanged(nameof(UnreadNotifications)); }
        }

        public ICommand NavigateDashboardCommand { get; set; }
        public ICommand NavigateAufgabenCommand {  get; set; }
        public ICommand NavigateKalenderCommand { get; set; }
        public ICommand NavigateStatistikCommand { get; set; }
        public ICommand NavigateKlassenCommand { get; set; }
        public ICommand NavigateGamificationCommand { get; set; }
        public ICommand NavigateNotificationsCommand { get; set; }
        public ICommand NavigateChatCommand { get; set; }

        public DashboardWindowViewModel()
        {
            var user = sessionManager.CurrentUser;
            Initials = user?.Initials ?? "?";
            DisplayName = user?.FullName ?? user?.UserName ?? "Schüler";
            RoleLevelText = user != null ? $"{user.Role} - Level {user.Level}" : "Schüler";

            NavigateDashboardCommand = new RelayCommand(()
                => CurrentPage = new App_LernPlaner.Views.DashboardPage());

            NavigateAufgabenCommand = new RelayCommand(()
                => CurrentPage = new App_LernPlaner.Views.AufgabenPage());

            NavigateKalenderCommand = new RelayCommand(()
                => CurrentPage = new App_LernPlaner.Views.KalenderPage());

            NavigateStatistikCommand = new RelayCommand(()
                => CurrentPage = new App_LernPlaner.Views.Statistics.StatistikPage());

            NavigateKlassenCommand = new RelayCommand(()
                => CurrentPage = new App_LernPlaner.Views.Classes.KlassenPage());

            NavigateGamificationCommand = new RelayCommand(()
                => CurrentPage = new App_LernPlaner.Views.Gamification.GamificationPage());

            NavigateNotificationsCommand = new RelayCommand(()
                => CurrentPage = new App_LernPlaner.Views.NotificationsPage());

            NavigateChatCommand = new RelayCommand(()
                => CurrentPage = new App_LernPlaner.Views.Chat.ChatPage());

            CurrentPage = new App_LernPlaner.Views.DashboardPage();

            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _pollTimer.Tick += async (_, _) => await RefreshUnreadNotificationsAsync();
            _pollTimer.Start();
            _ = RefreshUnreadNotificationsAsync();
        }

        // FA-25: Ungelesene Benachrichtigungen periodisch prüfen (kein WebSocket, einfaches Polling)
        private async Task RefreshUnreadNotificationsAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            try
            {
                UnreadNotifications = await _notificationService.CountUnreadAsync(user.UserId);
                await _notificationService.CheckDeadlineRemindersAsync(user);
            }
            catch
            {
                // Netzwerk-/DB-Fehler beim Hintergrund-Polling sollen die App nicht stören.
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
