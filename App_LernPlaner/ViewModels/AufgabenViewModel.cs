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
using System.Windows;
using System.Windows.Input;

namespace App_LernPlaner.ViewModels
{
    public class AufgabenViewModel : INotifyPropertyChanged

    {
        private readonly TaskService _taskService;
        private readonly int _UserId;
        public ObservableCollection<StudyTask> Aufgaben { get; set; }

        private List<StudyTask> _allTasks = new();

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

        private string _newTaskTitle;
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set
            {
                if (_newTaskTitle != value)
                {
                    _newTaskTitle = value;
                    OnPropertyChanged(nameof(NewTaskTitle));
                }
            }
        }

        private string _newTaskSubject;
        public string NewTaskSubject
        {
            get => _newTaskSubject;
            set
            {
                if (_newTaskSubject != value)
                {
                    _newTaskSubject = value;
                    OnPropertyChanged(nameof(NewTaskSubject));
                }
            }
        }

        private DateTime _newTaskDeadline = DateTime.Now;
        public DateTime NewTaskDeadline
        {
            get => _newTaskDeadline;
            set
            {
                if (_newTaskDeadline != value)
                {
                    _newTaskDeadline = value;
                    OnPropertyChanged(nameof(NewTaskDeadline));
                }
            }
        }
        private byte _newTaskIsRecurring;
        public byte NewTaskIsRecurring
        {
            get => _newTaskIsRecurring;
            set
            {
                if (_newTaskIsRecurring != value)
                {
                    _newTaskIsRecurring = value;
                    OnPropertyChanged(nameof(NewTaskIsRecurring));
                }
            }
        }

        private string _newTaskDescription;
        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set
            {
                if (_newTaskDescription != value)
                {
                    _newTaskDescription = value;
                    OnPropertyChanged(nameof(NewTaskDescription));
                }
            }
        }


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

        private bool _isAddTaskOpen;
        public bool IsAddTaskOpen
        {
            get => _isAddTaskOpen;
            set
            {
                if (_isAddTaskOpen != value)
                {
                    _isAddTaskOpen = value;
                    OnPropertyChanged(nameof(IsAddTaskOpen));
                }
            }
        }

        private int _newTaskPriorityIndex;
        public int NewTaskPriorityIndex
        {
            get => _newTaskPriorityIndex;
            set
            {
                if (_newTaskPriorityIndex != value)
                {
                    _newTaskPriorityIndex = value;
                    OnPropertyChanged(nameof(NewTaskPriorityIndex));
                }
            }
        }

        private string _searchText = string.Empty; 
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilter();
                }
            }
        }
        public byte NewTaskPriority => (byte)(NewTaskPriorityIndex + 1);

        public ICommand LoadAufgabenCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ICommand AddTaskCommand { get; }

        public ICommand CancelNewTaskCommand { get; }

        public ICommand SaveNewTaskCommand { get; }

        public AufgabenViewModel()
        {
            _taskService = new TaskService();

            _UserId = 1; // Kommt später
            Aufgaben = new ObservableCollection<StudyTask>();

            LoadAufgabenCommand = new AsyncRelayCommand(LoadAufgabeAsync);
            OpenTaskCommand = new RelayCommand(OpenTask);
            AddTaskCommand = new RelayCommand(AddTask);

            CancelNewTaskCommand = new RelayCommand(CancelNewTask);
            SaveNewTaskCommand = new AsyncRelayCommand(SaveNewTaskAsync);
        }

        private void ApplyFilter()
        {
            Aufgaben.Clear();

            var filteredTasks = _allTasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim().ToLower();

                filteredTasks = filteredTasks.Where(task => task.Titel.ToLower().Contains(search) 
                || task.Subject.ToLower().Contains(search) );

            }

            foreach (var task in filteredTasks)
            {
                Aufgaben.Add(task);
            }

            RefreshStats();
        }


        private async Task SaveNewTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                MessageBox.Show("Bitte Titel eingeben.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTaskSubject))
            {
                MessageBox.Show("Bitte Fach eingeben.");
                return;
            }

            if (NewTaskDeadline == null || NewTaskDeadline < DateTime.Now)
            {
                MessageBox.Show("Bitte Deadline eingeben.");
                return;
            }

            var task = new StudyTask
            {
                UserId = _UserId,
                Titel = NewTaskTitle.Trim(),
                Subject = NewTaskSubject.Trim(),
                Deadline = NewTaskDeadline,
                Priority = NewTaskPriority,
                Status = 0,
                Description = NewTaskDescription?.Trim(),
                IsRecurring = NewTaskIsRecurring == 1,
                ClassId = null,
                CreatedBy = _UserId,
                ButtonText = "Öffnen",

            };

            await _taskService.AddTaskAsync(task);

            IsAddTaskOpen = false;
            ClearNewTaskFrom();
            await LoadAufgabeAsync();
        }

        private void ClearNewTaskFrom()
        {
            NewTaskTitle = string.Empty;
            NewTaskSubject = string.Empty;
            NewTaskDeadline = DateTime.Now;
            NewTaskIsRecurring = 0;
            NewTaskDescription = string.Empty;
            NewTaskPriorityIndex = 0;
        }
        private async Task LoadAufgabeAsync()
        {

            _allTasks = await _taskService.GetTasksByUserIdAsync(_UserId);

            ApplyFilter();
        }

        private void OpenTask(object? parameter)
        {

        }

        private void AddTask(object? parameter)
        {
            IsAddTaskOpen = true;
        }

        private void CancelNewTask(object? parameter)
        {
            IsAddTaskOpen = false;
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
