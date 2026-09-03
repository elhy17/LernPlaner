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
    /// Interaktionslogik für KalenderPage.xaml
    /// </summary>
    public partial class KalenderPage : Page
    {
        private ViewModels.KalenderViewModel _viewModel;
        public KalenderPage()
        {
            InitializeComponent();
            _viewModel = new ViewModels.KalenderViewModel();
            DataContext = _viewModel;

            Loaded += (_, _) =>
            {
                if (_viewModel.LoadCalendarCommand.CanExecute(null))
                {
                    _viewModel.LoadCalendarCommand.Execute(null);
                }
            };
        }
    }
}
