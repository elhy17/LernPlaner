using App_LernPlaner.Commands;
using App_LernPlaner.Models;
using App_LernPlaner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace App_LernPlaner.ViewModels
{
    public class AufgabenViewModel : INotifyPropertyChanged

    {
        private readonly TaskService _taskService;
        private readonly int _UserId;

        public int AlleAufgaben => Aufgaben.Count;
        public int OffeneAufgaben => Aufgaben.Count(x => x.Status == 0);

        public int InBearbeitung => Aufgaben.Count(x => x.Status == 1);

        public int Erledigt => Aufgaben.Count(x => x.Status == 2);

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(AlleAufgaben));
            OnPropertyChanged(nameof(OffeneAufgaben));
            OnPropertyChanged(nameof(InBearbeitung));
            OnPropertyChanged(nameof(Erledigt));
        }


        public ObservableCollection<StudyTask> Aufgaben { get; set; }

        public ICommand LoadAufgabenCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ICommand AddTaskCommand { get; }

        private byte _progressStatus;
        public byte ProgressStatus
        {
            get => _progressStatus;
            set
            {
                if (_progressStatus != value)
                {
                    _progressStatus = value;
                    OnPropertyChanged(nameof(ProgressStatus));
                    OnPropertyChanged(nameof(ProgressStatusText));
                    OnPropertyChanged(nameof(OffenButtonBackground));
                    OnPropertyChanged(nameof(InBearbeitungButtonBackground));
                    OnPropertyChanged(nameof(ErledigtButtonBackground));
                    OnPropertyChanged(nameof(OffenButtonBorder));
                    OnPropertyChanged(nameof(InBearbeitungButtonBorder));
                    OnPropertyChanged(nameof(ErledigtButtonBorder));
                }
            }
        }
        public AufgabenViewModel()
        {
            _taskService = new TaskService();

            _UserId = 1; // Kommt später
            Aufgaben = new ObservableCollection<StudyTask>();

            LoadAufgabenCommand = new AsyncRelayCommand(LoadAufgabeAsync);
            OpenTaskCommand = new RelayCommand(OpenTask);
            AddTaskCommand = new RelayCommand(AddTask);
        }

        private async Task LoadAufgabeAsync()
        {
            Aufgaben.Clear();

            var tasks = await _taskService.GetTasksByUserIdAsync(_UserId);

            foreach (var task in tasks)
            {
                Aufgaben.Add(task);
            }

            RefreshStats();
        }

        private void OpenTask(object? parameter)
        {

        }

        private void AddTask(object? parameter)
        {

        }

        public string OffenButtonBackground =>
           ProgressStatus == 0 ? "#2563EB" : "#1F2937";

        public string InBearbeitungButtonBackground =>
            ProgressStatus == 1 ? "#2563EB" : "#1F2937";
        public string ErledigtButtonBackground =>
            ProgressStatus == 2 ? "#2563EB" : "#1F2937";

        public string OffenButtonBorder =>
            ProgressStatus == 0 ? "#2563EB" : "#334155";

        public string InBearbeitungButtonBorder =>
            ProgressStatus == 1 ? "#2563EB" : "#334155";

        public string ErledigtButtonBorder =>
            ProgressStatus == 2 ? "#2563EB" : "#334155";


        public string ProgressStatusText => ProgressStatus switch
        {
                0 => "Offen",
                1 => "In Bearbeitung",
                2 => "Erledigt",
                _ => "Unbekannt"
        };


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        }
    }
}
