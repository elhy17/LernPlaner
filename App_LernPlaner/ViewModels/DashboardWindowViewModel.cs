using App_LernPlaner.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace App_LernPlaner.ViewModels
{
    public class DashboardWindowViewModel : INotifyPropertyChanged
    {
        private Page? _currentPage;

        public Page? CurrentPage
        {
            get => _currentPage;
            set
            {
               _currentPage = value;
              OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public ICommand NavigateDashboardCommand { get; set; }

        public ICommand NavigateAufgabenCommand {  get; set; }


        public DashboardWindowViewModel()
        {
            NavigateDashboardCommand = new RelayCommand(() 
                => CurrentPage = new App_LernPlaner.Views.DashboardPage());

            NavigateAufgabenCommand = new RelayCommand(() 
                => CurrentPage = new App_LernPlaner.Views.AufgabenPage());

            CurrentPage = new App_LernPlaner.Views.DashboardPage();

           
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
