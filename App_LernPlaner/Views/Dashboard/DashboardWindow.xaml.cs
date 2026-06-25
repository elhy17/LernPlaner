using App_LernPlaner.Database;
using App_LernPlaner.Models;
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
        public DashboardWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new App_LernPlaner.Views.DashboardPage());
        }

        private void Aufgaben_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new App_LernPlaner.Views.AufgabenPage());
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new App_LernPlaner.Views.DashboardPage());
        }
    }
}
