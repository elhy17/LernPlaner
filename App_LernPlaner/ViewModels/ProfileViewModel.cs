using App_LernPlaner.Commands;
using App_LernPlaner.Models;
using App_LernPlaner.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using App_LernPlaner.Views;

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

        private bool _isChangePasswordOpen;
        public bool IsChangePasswordOpen
        {
            get => _isChangePasswordOpen;
            set
            {
                if (_isChangePasswordOpen != value)
                {
                    _isChangePasswordOpen = value;
                    OnPropertyChanged(nameof(IsChangePasswordOpen));
                }
            }
        }

        public ICommand EditProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand CancelChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand SaveProfileCommand { get; }
        public ICommand CancelEditProfileCommand { get; }
        public ICommand LoadProfileCommand { get; }

        public ProfileViewModel()
        {
            LoadProfileCommand = new AsyncRelayCommand(LoadProfileAsync);
            EditProfileCommand = new RelayCommand(EditProfile);
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            CancelChangePasswordCommand = new RelayCommand(CancelChangePassword);
            LogoutCommand = new AsyncRelayCommand(LogoutAsync);

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

            try
            {
                var fresh = await _users.GetByIdAsync(user.UserId);
                if (fresh != null)
                {
                    sessionManager.CurrentUser = fresh;
                    user = fresh;
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

                    // FA-32: eigene Platzierung in der ersten Klasse, in der man Mitglied ist
                    var classes = await new ClassService().GetClassesForUserAsync(user.UserId);
                    var firstClass = classes.FirstOrDefault();
                    if (firstClass != null)
                    {
                        var leaderboard = await new GamificationService().GetLeaderboardAsync(firstClass.ClassId);
                        int rank = leaderboard.FindIndex(u => u.UserId == user.UserId) + 1;
                        RankText = rank > 0 ? $"Rang {rank} in {firstClass.Name}" : string.Empty;
                    }
                    else
                    {
                        RankText = string.Empty;
                    }
                }
                else
                {
                    RankText = string.Empty;
                }

                OnPropertyChanged(string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Profil konnte nicht geladen werden.\n\n" + ex.Message);
            }
        }

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(EditFirstName) || string.IsNullOrWhiteSpace(EditLastName) || string.IsNullOrWhiteSpace(EditEmail))
            {
                MessageBox.Show("Bitte Vor- und Nachname sowie E-Mail ausfüllen.");
                return;
            }

            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var profile = new UserProfile
            {
                UserId = user.UserId,
                FirstName = EditFirstName.Trim(),
                LastName = EditLastName.Trim(),
                Email = EditEmail.Trim()
            };

            await _users.UpdateUserProfileAsync(profile);

            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Email = profile.Email;
            FullName = $"{FirstName} {LastName}".Trim();
            Initials = (FirstName.Substring(0, 1) + LastName.Substring(0, 1)).ToUpper();

            user.FirstName = FirstName;
            user.LastName = LastName;
            user.Email = Email;

            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Initials));

            IsEditProfileOpen = false;
            MessageBox.Show("Profil wurde gespeichert.");
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

        // FA-06: eigenes Passwort ändern (im Gegensatz zu "Passwort vergessen" auf der Login-Seite,
        // das ein neues temporäres Passwort vergibt, prüft dieser Weg das aktuelle Passwort).
        private void ChangePassword(object? parameter)
        {
            IsChangePasswordOpen = true;
        }

        private void CancelChangePassword(object? parameter)
        {
            IsChangePasswordOpen = false;
        }

        public void OnPasswordChanged()
        {
            IsChangePasswordOpen = false;
        }

        // FA-02: ausloggen - beendet die Session und kehrt zur Anmeldeseite zurück.
        private async Task LogoutAsync()
        {
            var result = MessageBox.Show("Möchtest du dich wirklich abmelden?", "Abmelden", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            var oldWindow = Application.Current.MainWindow;

            await sessionManager.LogoutAsync();

            var login = new LoginPage();
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
