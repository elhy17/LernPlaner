using App_LernPlaner.Commands;
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

namespace App_LernPlaner.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
       {

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
        }

        private async Task LoadProfileAsync()
        {
            await Task.CompletedTask;
        }

        private void SaveProfile(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(EditFirstName) || string.IsNullOrWhiteSpace(EditLastName) || string.IsNullOrWhiteSpace(EditEmail))
            {
                MessageBox.Show("Bitte alle Felder ausfüllen.");
                return;
            }

            FirstName = EditFirstName;
            LastName = EditLastName;
            Email = EditEmail;

            FullName = FirstName + " " + LastName;
            Initials = FirstName.Substring(0, 1).ToUpper() + LastName.Substring(0,1).ToUpper();

            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(Email));
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

        private void ChangePassword(object? parameter)
        {
            MessageBox.Show("Change Password clicked");
        }

        private void Logout(object? parameter)
        {
            MessageBox.Show("Logout clicked");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
