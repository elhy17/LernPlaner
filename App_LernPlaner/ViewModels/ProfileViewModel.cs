using App_LernPlaner.Commands;
using App_LernPlaner.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace App_LernPlaner.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
       {

        public event Action? LogoutRequested;

        private readonly Services.UserService _userService;
        private readonly int _currentUserId = 1;
        public string Initials { get; set; } = "MS";
        public string FullName { get; set; } = "Max Schmidt";
        public string RoleAndClass { get; set; } = "Schüler - Klasse 10A";

        public string LevelText { get; set; } = "Level 3";
        public string XpText { get; set; } = "XP 680";
        public string RankText { get; set; } = "Rang 3";

        public string FirstName { get; set; } = "Max";
        public string LastName { get; set; } = "Schmidt";
        public string UserName { get; set; } = "max.Schmidt";
        public string Email { get; set; } = "Max.Schmidt@schule.de";
        public string Role { get; set; } = "Schüler";

        public int OpenTasks { get; set; } = 5;
        public int FinishedTasks { get; set; } = 18;
        public string AverageGrade { get; set; } = "2,3";
        public int TotalXp { get; set; } = 680;

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
        public ICommand LogoutCommand { get; }

        public ICommand SaveProfileCommand { get; }
        public ICommand CancelEditProfileCommand { get; }

        public ICommand LoadProfileCommand { get; }
        public ICommand CancelChangePasswordCommand { get; }
        public ProfileViewModel()
        {
            _userService = new Services.UserService();


            LoadProfileCommand = new AsyncRelayCommand(LoadProfileAsync);
            EditProfileCommand = new RelayCommand(EditProfile);
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            LogoutCommand = new RelayCommand(Logout);

            CancelEditProfileCommand = new RelayCommand(CancelEditProfile);
            SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync);
            CancelChangePasswordCommand = new RelayCommand(CancelChangePassword);


        }

        private void ChangePassword(object? parameter)
        {
            IsChangePasswordOpen = true;
        }

        private void CancelChangePassword(object? parameter)
        {
            IsChangePasswordOpen = false;
        }   


        public async Task LoadProfileAsync()
        {
            try
            {
                UserProfile? userProfile = await _userService.GetUserProfileByIdAsync(_currentUserId);

                if(userProfile == null)
                {
                    MessageBox.Show("Profil wurde nicht gefunden.");
                    return;
                }

                Initials = userProfile.Initials;
                FullName = string.IsNullOrWhiteSpace(userProfile.FullName)
                ? userProfile.FullName : userProfile.FullName;
                RoleAndClass = userProfile.RoleAndClass;
                LevelText = $"Level {userProfile.Level}";
                RankText = "Rang 3";

                FirstName = userProfile.FirstName;
                LastName = userProfile.LastName;
                UserName = userProfile.Username;
                Email = userProfile.Email;
                Role = userProfile.Role;

                TotalXp = userProfile.XPPoints;

                OnPropertyChanged(nameof(Initials));
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(RoleAndClass));
                OnPropertyChanged(nameof(LevelText));
                OnPropertyChanged(nameof(XpText));
                OnPropertyChanged(nameof(RankText));
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(UserName));
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(Role));
                OnPropertyChanged(nameof(TotalXp));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Profil konnte nicht geladen werden.\n\n" + ex.Message);
            }
        }


        private async Task SaveProfileAsync()
        {
            if(string.IsNullOrWhiteSpace(EditEmail) || string.IsNullOrWhiteSpace(EditFirstName) || string.IsNullOrWhiteSpace(EditLastName))
            {
                MessageBox.Show("Bitte Alle Felder Ausfüllen");
                return;
            }

            var profile = new UserProfile
            {
                UserId = _currentUserId,
                Email = EditEmail.Trim(),
                FirstName = EditFirstName.Trim(),
                LastName = EditLastName.Trim(),
            };

            await _userService.UpdateUserProfile(profile);

            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Email = profile.Email;
            FullName = FirstName + " " + LastName;
            Initials = FirstName.Substring(0, 1).ToUpper() + LastName.Substring(0, 1).ToUpper();

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


        private void Logout(object? parameter)
        {
            var result = MessageBox.Show("Möchten Sie sich wirklich abmelden?", "Abmelden", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(result == MessageBoxResult.Yes)
            {
                LogoutRequested?.Invoke();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
