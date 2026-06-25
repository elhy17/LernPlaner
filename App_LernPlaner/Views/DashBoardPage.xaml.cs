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
            txtDatum.Text = DateTime.Now.ToString("ddd, dd. MMMM yyyy");
            DataContext = new ViewModels.DashboardViewModel();
        }
    }
}
