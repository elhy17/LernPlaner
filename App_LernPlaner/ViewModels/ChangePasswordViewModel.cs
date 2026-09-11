using App_LernPlaner.Commands;
using App_LernPlaner.Services;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace App_LernPlaner.ViewModels
{
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmNewPassword = string.Empty;
        private bool _isBusy;

        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                if (_currentPassword != value)
                {
                    _currentPassword = value;
                    OnPropertyChanged(nameof(CurrentPassword));
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (_newPassword != value)
                {
                    _newPassword = value;
                    OnPropertyChanged(nameof(NewPassword));
                }
            }
        }

        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set
            {
                if (_confirmNewPassword != value)
                {
                    _confirmNewPassword = value;
                    OnPropertyChanged(nameof(ConfirmNewPassword));
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        public ICommand SavePasswordCommand { get; }
        public ICommand CancelCommand { get; }

        public ChangePasswordViewModel()
        {
            _authService = new AuthService();
            SavePasswordCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(ClearPasswords);
        }

        public async Task<bool> SaveAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null)
            {
                MessageBox.Show(
                    "Sie sind nicht angemeldet.",
                    "Keine Sitzung",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                MessageBox.Show(
                    "Bitte fuellen Sie alle Passwortfelder aus.",
                    "Eingabe unvollstaendig",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (NewPassword.Length < 8)
            {
                MessageBox.Show(
                    "Das neue Passwort muss mindestens 8 Zeichen lang sein.",
                    "Passwort zu kurz",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                MessageBox.Show(
                    "Das neue Passwort und die Bestaetigung stimmen nicht ueberein.",
                    "Passwoerter unterschiedlich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (CurrentPassword == NewPassword)
            {
                MessageBox.Show(
                    "Das neue Passwort muss sich vom aktuellen Passwort unterscheiden.",
                    "Passwort unveraendert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            IsBusy = true;
            try
            {
                var result = await _authService.TryChangePasswordAsync(
                    user.UserId,
                    CurrentPassword,
                    NewPassword);

                if (!result.Success)
                {
                    MessageBox.Show(
                        result.Error,
                        "Passwort aendern fehlgeschlagen",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return false;
                }

                ClearPasswords();

                MessageBox.Show(
                    "Passwort wurde erfolgreich geaendert.",
                    "Erfolgreich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ClearPasswords()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
