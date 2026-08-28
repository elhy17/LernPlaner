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
        private readonly SubmissionService _submissionService;
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

        private bool _isEditingTaskProgress;
        public bool IsEditingTaskProgress
        {
            get => _isEditingTaskProgress;
            set
            {
                _isEditingTaskProgress = value;
                OnPropertyChanged(nameof(IsEditingTaskProgress));
                OnPropertyChanged(nameof(DetailViewVisibility));
                OnPropertyChanged(nameof(EditViewVisibility));
            }
        }

        private string _progressNotes = string.Empty;
        public string ProgressNotes
        {
            get => _progressNotes;
            set
            {
                _progressNotes = value;
                OnPropertyChanged(nameof(ProgressNotes));
            }
        }

        private string _ProgressAnswer = string.Empty;
        public string ProgressAnswer
        {
            get => _ProgressAnswer;
            set
            {
                _ProgressAnswer = value;
                OnPropertyChanged(nameof(ProgressAnswer));
            }
        }

        private int _learningMinutes;
        public int LearningMinutes
        {
            get => _learningMinutes;
            set
            {
                _learningMinutes = value;
                OnPropertyChanged(nameof(LearningMinutes));
            }
        }

        public Visibility DetailViewVisibility => 
            IsEditingTaskProgress ? Visibility.Collapsed: Visibility.Visible;

        public Visibility EditViewVisibility =>
            IsEditingTaskProgress ? Visibility.Visible : Visibility.Collapsed;

        private byte _progressStatus;
        private byte ProgressStatus
        {
            get => _progressStatus;
            set
            {
                _progressStatus = value;
                OnPropertyChanged(nameof(ProgressStatus));

                OnPropertyChanged(nameof(OffenButtonBackground));
                OnPropertyChanged(nameof(InBearbeitungButtonBackground));
                OnPropertyChanged(nameof(ErledigtButtonBackground));

                OnPropertyChanged(nameof(OffenButtonBorder));
                OnPropertyChanged(nameof(InBearbeitungButtonBorder));
                OnPropertyChanged(nameof(ErledigtButtonBorder));
            }
        }

        private string _submissionFilePath = string.Empty;
        public string SubmissionFilePath
        {
            get => _submissionFilePath;
            set
            {
                _submissionFilePath = value;
                OnPropertyChanged(nameof(SubmissionFilePath));
            }
        }

        public ICommand LoadDashboardCommand { get;  }
        public ICommand RefreshCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ICommand CloseTaskDetailsCommand { get; }

        // Dieser Command für EditingTaskProgress
        public ICommand EditTaskProgressCommand { get; }
        public ICommand CancelEditTaskProgressCommand { get; }
        public ICommand SaveTaskProgressCommand { get; }
        public ICommand SetProgressStatusCommand { get; }

        public ICommand ChooseSubmissionFileCommand { get; }
        public ICommand MarkTaskAsDoneCommand { get; }


        public DashboardViewModel()
        {
            _taskService = new TaskService();
            _submissionService = new SubmissionService();

            var currentUser = sessionManager.CurrentUser;
            _currentUserId = currentUser?.UserId ?? 0;
            Aufgaben = new ObservableCollection<Models.StudyTask>();

            GreettingText = $"Guten Morgen, {currentUser?.FirstName ?? currentUser?.UserName ?? "Schüler"} 👋";

            CurrentDateText = DateTime.Now.ToString("ddd, dd. MMMM yyyy", new CultureInfo("de-DE"));

            Durchschnittsnote = "-";

            XpGesamt = currentUser?.XPPoints ?? 0;

            LoadDashboardCommand = new Commands.AsyncRelayCommand(LoadDashboardAsync);
            RefreshCommand = new Commands.AsyncRelayCommand(LoadDashboardAsync);
            OpenTaskCommand = new Commands.RelayCommand(OpenTask);
            CloseTaskDetailsCommand = new RelayCommand(CloseTaskDestails);

            EditTaskProgressCommand = new RelayCommand(EditTaskProgress);
            CancelEditTaskProgressCommand = new RelayCommand(CancelEditTaskProgress);
            SaveTaskProgressCommand = new AsyncRelayCommand(SaveTaskProgressAsync);

            SetProgressStatusCommand = new RelayCommand(SetProgressStatus);

            ChooseSubmissionFileCommand = new RelayCommand(ChooseSubmissionFile);
            
            MarkTaskAsDoneCommand = new AsyncRelayCommand(MarkTaskAsDoneAsync);
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

                if (sessionManager.CurrentUser != null)
                {
                    var stats = await new StatisticsService().GetStudentStatsAsync(sessionManager.CurrentUser);
                    Durchschnittsnote = stats.Durchschnittsnote;
                    XpGesamt = stats.XpGesamt;
                }
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
            IsEditingTaskProgress = false;
        }


        // <--  Für EditTask   -->
        
        private void EditTaskProgress(object? parameter)
        {
            if(SelectedTask == null)
            {
                return;
            }


            IsEditingTaskProgress = true;
        }

        private void CancelEditTaskProgress(object? parameter)
        {
            IsEditingTaskProgress = false;
        }

        private async Task SaveTaskProgressAsync()
        {
            if(SelectedTask == null)
            {
                MessageBox.Show("Keine Aufgabe ausgewählt.");
                return;
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ProgressAnswer))
            {
                errors.Add("Abgabe / Antwort fehlt.");
            }

            if (LearningMinutes <= 0)
            {
                errors.Add("Lernzeit muss größer als 0 sein.");
            }

            if (errors.Any())
            {
                MessageBox.Show(
                    "Bitte prüfen Sie Ihre Eingaben:\n\n" + string.Join("\n", errors),
                    "Eingaben unvollständig",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            await _submissionService.SaveProgressAsync(
                SelectedTask.TaskId,
                _currentUserId,
                ProgressStatus,
                ProgressNotes,
                ProgressAnswer,
                LearningMinutes,
                SubmissionFilePath);

            await LoadDashboardAsync();

            MessageBox.Show(
            "Der Lernfortschritt wurde erfolgreich gespeichert. Ihre Abgabe wurde an den zuständigen Lehrer übermittelt.",
            "Vorgang erfolgreich abgeschlossen",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

            IsEditingTaskProgress = false;
        }

        private void SetProgressStatus(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }

            ProgressStatus = Convert.ToByte(parameter);
        }

        // Chose die Datei
        private void ChooseSubmissionFile(object? parameter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Datei für Abgabe ausWählen",
                Filter = "Alle Dateien (*.*)|*.*|PDF (*.pdf)|*.pdf|Word (*.docx)|*.docx|Bilder (*.png;*.jpg)|*.png;*.jpg"
            };

            if (dialog.ShowDialog() == true)
            {
                SubmissionFilePath = dialog.FileName;
            }
        }

        // => Markieren Als Erledigt
        private async Task MarkTaskAsDoneAsync()
        {
            if(SelectedTask == null)
            {
                MessageBox.Show("Keine Aufgaben ausgewählt.");
                return;
            }

            ProgressStatus = 2;

            await _submissionService.SaveProgressAsync(
                SelectedTask.TaskId,
                _currentUserId,
                ProgressStatus,
                ProgressNotes,
                ProgressAnswer,
                LearningMinutes,
                SubmissionFilePath);

            IsTaskDetailsOpen = false;
            IsEditingTaskProgress = false;

            await LoadDashboardAsync();

            MessageBox.Show(
            "Die Aufgabe wurde erfolgreich als erledigt markiert und an den zuständigen Lehrer übermittelt.",
            "Erfolgreich",
            MessageBoxButton.OK,
            MessageBoxImage.Information);


        }

        // <--  Für Farben  -->
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


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}