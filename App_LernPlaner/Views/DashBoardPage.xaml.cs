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
using System.Windows.Shapes;

namespace App_LernPlaner.Views
{
    /// <summary>
    /// Interaktionslogik für DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            var viewModel = new ViewModels.DashboardViewModel();
            DataContext = viewModel;

            Loaded += (_, _) =>
            {
                if (viewModel.LoadDashboardCommand.CanExecute(null))
                {
                    viewModel.LoadDashboardCommand.Execute(null);
                }
            };

            Unloaded += (_, _) => viewModel.StopPolling();
        }
    }
}
