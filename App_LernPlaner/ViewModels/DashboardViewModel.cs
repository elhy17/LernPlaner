using App_LernPlaner.Commands;
using App_LernPlaner.Models;
using App_LernPlaner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace App_LernPlaner.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly Services.TaskService _taskService;
        private readonly int _currentUserId;
        public ObservableCollection<Models.StudyTask> Aufgaben { get; set; }


        private string _grettingText;
        public string GreettingText
        {
            get => _grettingText;
            set
            {
                _grettingText = value;
                OnPropertyChanged(nameof(GreettingText));
            }
        }

        private string _currentDateText;
        public string CurrentDateText
        {
            get => _currentDateText;
            set
            {
                _currentDateText = value;
                OnPropertyChanged(nameof(CurrentDateText));
            }
        }


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

        private int _abgegeben;
        public int Abgegeben
        {
            get => _abgegeben;
            set
            {
                _abgegeben = value;
                OnPropertyChanged(nameof(Abgegeben));
            }
        }

        private int _faelligDieseWoche;
        public int FaelligDieseWoche
        {
            get => _faelligDieseWoche;
            set
            {
                _faelligDieseWoche = value;
                OnPropertyChanged(nameof(FaelligDieseWoche));
            }
        }

        private string _durchschnittsnote;
        public string Durchschnittsnote
        {
            get => _durchschnittsnote;
            set
            {
                _durchschnittsnote = value;
                OnPropertyChanged(nameof(Durchschnittsnote));
            }
        }

        private int _xpGesamt;
        public int XpGesamt
        {
            get => _xpGesamt;
            set
            {
                _xpGesamt = value;
                OnPropertyChanged(nameof(XpGesamt));
            }
        }

        private StudyTask? _selectedTask;
        public StudyTask? SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged(nameof(SelectedTask));
            }
        }

        private bool _isTaskDetailsOpen;
        public bool IsTaskDetailsOpen
        {
            get => _isTaskDetailsOpen;
            set
            {
                _isTaskDetailsOpen = value;
                OnPropertyChanged(nameof(IsTaskDetailsOpen));
            }
        }

        public ICommand LoadDashboardCommand { get;  }
        public ICommand RefreshCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ICommand CloseTaskDetailsCommand { get; }



        public DashboardViewModel()
        {
            _taskService = new TaskService();

            _currentUserId = 1; // Hier kommt user später bei Login
            Aufgaben = new ObservableCollection<Models.StudyTask>();

           GreettingText = "Guten Morgen, Amine 👋";

            CurrentDateText = DateTime.Now.ToString("ddd, dd. MMMM yyyy", new CultureInfo("de-DE"));

            Durchschnittsnote = "2,3";

            XpGesamt = 680;

            LoadDashboardCommand = new Commands.AsyncRelayCommand(LoadDashboardAsync);
            RefreshCommand = new Commands.AsyncRelayCommand(LoadDashboardAsync);
            OpenTaskCommand = new Commands.RelayCommand(OpenTask);
            CloseTaskDetailsCommand = new RelayCommand(CloseTaskDestails);

        }

        private async Task LoadDashboardAsync()
        {
            try
            {
                var tasks = await _taskService.GetTasksByUserIdAsync(_currentUserId); 

                Aufgaben.Clear();

                foreach (var task in tasks)
                {
                    Aufgaben.Add(task);
                }

                OffeneAufgaben = _taskService.CountOpenTasks(tasks);
                Abgegeben = _taskService.CountFinishedTasks(tasks);
                FaelligDieseWoche = _taskService.CountDueThisWeek(tasks);

            }
            catch (Exception ex)
            {
               MessageBox.Show("Daten konnten nicht geladen werden: " + ex.Message);
            }


        }

         private void OpenTask(object? parameter)
         {


            if (parameter is not StudyTask task)
            {
                MessageBox.Show("Keine Aufgabe gefunden.");
                return;
            }

            SelectedTask = task;
            IsTaskDetailsOpen = true;

        }

        private void CloseTaskDestails()
        {
            IsTaskDetailsOpen = false;
        }
 

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}