using App_LernPlaner.ViewModels;
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

namespace App_LernPlaner.Views
{
    /// <summary>
    /// Interaktionslogik für ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            var viewModel = new ProfileViewModel();
            DataContext = viewModel;

            Loaded += (_, _) =>
            {
                if (viewModel.LoadProfileCommand.CanExecute(null))
                {
                    viewModel.LoadProfileCommand.Execute(null);
                }
            };
        }

        private void Logout()
        {
            // Später: Implementiere die Logout-Logik hier   
        }
    }
}
