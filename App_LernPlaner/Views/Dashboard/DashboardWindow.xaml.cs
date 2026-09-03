using App_LernPlaner.Database;
using App_LernPlaner.Models;
using App_LernPlaner.ViewModels;
using MySql.Data.MySqlClient;
using Nexus_Team;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace App_LernPlaner.Views.Dashboard
{

    public partial class DashboardWindow : Window
    {
        private DashboardWindowViewModel _viewModel;
        public DashboardWindow()
        {
            InitializeComponent();
            _viewModel = new DashboardWindowViewModel();
            DataContext = _viewModel;

        }

        private void Profil_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new App_LernPlaner.Views.ProfilePage());
        }

    }
}
