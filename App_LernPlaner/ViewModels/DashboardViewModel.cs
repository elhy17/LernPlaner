using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly Services.TaskService _taskService;
        public ObservableCollection<Models.Task> Aufgaben { get; set; }
        private int _offeneAufgaben;
        public int OffeneAufgaben
        {
            get => _offeneAufgaben;
            set
            {
                _offeneAufgaben = value;
                OnPropertyChanged(nameof(OffeneAufgaben));
            }
        }

        public DashboardViewModel()
        {
            _taskService = new TaskService();
            Aufgaben = new ObservableCollection<Models.Task>();
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                Aufgaben.Clear();
                var tasks = await _taskService.GetTasksAsync();
                foreach (var task in tasks)
                {
                    Aufgaben.Add(task);
                }

                OffeneAufgaben = await _taskService.GetOpenTaskCountAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Daten konnten nicht geladen werden.\n\n" +
                    "Bitte prüfe, ob der Datenbankserver (MySQL) läuft.\n\n" +
                    "Details: " + ex.Message,
                    "Verbindungsfehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}