using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using App_LernPlaner.Services;
using App_LernPlaner.Views;

namespace Nexus_Team.views
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnRecover_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RecoveryPage());
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string uN  = txtUsername.Text.ToLower().Trim();
            string passwordNotHashed = txtPassword.Password;
            if (string.IsNullOrEmpty(uN) || string.IsNullOrEmpty(passwordNotHashed)) MessageBox.Show("ungutige eingaben!");

            else 
            {
                var login = new logIn(uN, passwordNotHashed);

                if (login.IsSuccessful == true)
                {

                    // NavigationService.Navigate(new DashBoardPage());
                }
            }


        }

        private void BtnGoToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }

       
    }
}