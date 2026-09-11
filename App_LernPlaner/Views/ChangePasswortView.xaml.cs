using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.ViewModels;

namespace App_LernPlaner.Views
{
    // FA-06: eigenes Passwort ändern (für Schüler, Lehrer und Admin gleichermaßen,
    // da diese UserControl von ProfilePage für alle Rollen verwendet wird).
    public partial class ChangePasswortView : UserControl
    {
        private readonly ChangePasswordViewModel _viewModel;

        public ChangePasswortView()
        {
            InitializeComponent();
            _viewModel = new ChangePasswordViewModel();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CurrentPassword = CurrentPasswordBox.Password;
            _viewModel.NewPassword = NewPasswordBox.Password;
            _viewModel.ConfirmNewPassword = ConfirmPasswordBox.Password;

            BtnSave.IsEnabled = false;

            try
            {
                bool success = await _viewModel.SaveAsync();

                if (!success)
                    return;

                CurrentPasswordBox.Password = string.Empty;
                NewPasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;

                if (DataContext is ProfileViewModel vm)
                    vm.OnPasswordChanged();
            }
            finally
            {
                BtnSave.IsEnabled = true;
            }
        }
    }
}
