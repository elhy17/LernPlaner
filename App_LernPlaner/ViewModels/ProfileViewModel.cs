using App_LernPlaner.Commands;
using App_LernPlaner.Services;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace App_LernPlaner.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly Repositories.UserRepository _users = new();

        public string Initials { get; set; } = "?";
        public string FullName { get; set; } = string.Empty;
        public string RoleAndClass { get; set; } = string.Empty;

        public string LevelText { get; set; } = string.Empty;
        public string XpText { get; set; } = string.Empty;
        public string RankText { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public int OpenTasks { get; set; }
        public int FinishedTasks { get; set; }
        public string AverageGrade { get; set; } = "-";
        public int TotalXp { get; set; }

        public string EditFirstName { get; set; } = string.Empty;
        public string EditLastName { get; set; } = string.Empty;
        public string EditEmail { get; set; } = string.Empty;

        private bool _isEditProfileOpen;
        public bool IsEditProfileOpen
        {
            get => _isEditProfileOpen;
            set
            {
                if (_isEditProfileOpen != value)
                {
                    _isEditProfileOpen = value;
                    OnPropertyChanged(nameof(IsEditProfileOpen));
                }
            }
        }


        public ICommand EditProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand SaveProfileCommand { get; }
        public ICommand CancelEditProfileCommand { get; }

        public ProfileViewModel()
        {
            EditProfileCommand = new RelayCommand(EditProfile);
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            LogoutCommand = new RelayCommand(Logout);

            CancelEditProfileCommand = new RelayCommand(CancelEditProfile);
            SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync);

            LoadFromSession();
            _ = LoadProfileAsync();
        }

        private void LoadFromSession()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            FirstName = user.FirstName ?? string.Empty;
            LastName = user.LastName ?? string.Empty;
            UserName = user.UserName;
            Email = user.Email;
            Role = user.Role;
            FullName = user.FullName;
            Initials = user.Initials;
            RoleAndClass = string.IsNullOrWhiteSpace(user.ClassName) ? user.Role : $"{user.Role} - Klasse {user.ClassName}";
            LevelText = $"Level {user.Level}";
            XpText = $"XP {user.XPPoints}";
            TotalXp = user.XPPoints;

            OnPropertyChanged(string.Empty);
        }

        // FA-27: echte Zahlen statt Platzhalter, sobald die Daten geladen sind
        private async Task LoadProfileAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var fresh = await _users.GetByIdAsync(user.UserId);
            if (fresh != null)
            {
                sessionManager.CurrentUser = fresh;
                TotalXp = fresh.XPPoints;
                XpText = $"XP {fresh.XPPoints}";
                LevelText = $"Level {fresh.Level}";
            }

            if (user.IsStudent)
            {
                var stats = await new StatisticsService().GetStudentStatsAsync(user);
                OpenTasks = stats.OffeneAufgaben;
                FinishedTasks = stats.Abgegeben;
                AverageGrade = stats.Durchschnittsnote;
            }

            OnPropertyChanged(string.Empty);
        }

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(EditFirstName) || string.IsNullOrWhiteSpace(EditLastName))
            {
                MessageBox.Show("Bitte Vor- und Nachname ausfüllen.");
                return;
            }

            var user = sessionManager.CurrentUser;
            if (user == null) return;

            await _users.UpdateProfileAsync(user.UserId, EditFirstName.Trim(), EditLastName.Trim());

            FirstName = EditFirstName.Trim();
            LastName = EditLastName.Trim();
            FullName = $"{FirstName} {LastName}".Trim();
            Initials = (FirstName.Substring(0, 1) + LastName.Substring(0, 1)).ToUpper();

            user.FirstName = FirstName;
            user.LastName = LastName;

            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Initials));

            IsEditProfileOpen = false;
        }

        private void EditProfile(object? parameter)
        {
            EditFirstName = FirstName;
            EditLastName = LastName;
            EditEmail = Email;

            OnPropertyChanged(nameof(EditFirstName));
            OnPropertyChanged(nameof(EditLastName));
            OnPropertyChanged(nameof(EditEmail));

            IsEditProfileOpen = true;
        }

        private void CancelEditProfile(object? parameter)
        {
            IsEditProfileOpen = false;
        }

        private void ChangePassword(object? parameter)
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            MessageBox.Show(
                "Bitte nutze \"Passwort vergessen?\" auf der Anmeldeseite, um dein Passwort zurückzusetzen.",
                "Passwort ändern", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // FA-02: ausloggen - beendet die Session und kehrt zur Anmeldeseite zurück.
        private void Logout(object? parameter)
        {
            var oldWindow = Application.Current.MainWindow;

            sessionManager.Logout();

            var login = new Nexus_Team.MainWindow();
            Application.Current.MainWindow = login;
            login.Show();

            oldWindow?.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
