using App_LernPlaner.Models;
using App_LernPlaner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace App_LernPlaner.ViewModels
{
    public class KalenderViewModel : INotifyPropertyChanged
    {
        private readonly Services.TaskService _taskService;

        private List<Models.StudyTask> _alltasks = new();
        private DateTime _currentMonth;

        public ObservableCollection<Models.CalendarDay> CalendarDays { get; set; } = new();
        public ObservableCollection<Models.StudyTask> UpcomingTasks { get; set; } = new();

        public ICommand LoadCalendarCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand TodayCommand { get; }

        public KalenderViewModel()
        {
            _taskService = new Services.TaskService();

            _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Today.Month, 1);

            LoadCalendarCommand = new Commands.AsyncRelayCommand(LoadClendarAsync);
            PreviousMonthCommand = new Commands.RelayCommand(PreviousMonth);
            NextMonthCommand = new Commands.RelayCommand(NextMonth);
            TodayCommand = new Commands.RelayCommand(GoToToday);
        }

        public string MonthTitle =>
            _currentMonth.ToString("MMMM yyyy", new CultureInfo("de-DE"));
        public int TodayTaskCount =>
            _alltasks.Count(task => task.Deadline.Date == DateTime.Today);

        public int WeekTaskCount
        {
            get
            {
                DateTime today = DateTime.Today;
                DateTime nextweek = today.AddDays(7);

                return _alltasks.Count(
                    task => task.Status != 2 &&
                    task.Deadline.Date >= today &&
                    task.Deadline.Date <= nextweek
                    );
            }
        }

        public int overdueTaskCount =>
            _alltasks.Count(task => task.Status != 2 && task.Deadline.Date < DateTime.Today);

        private async Task LoadClendarAsync()
        {
            try
            {
                var user = sessionManager.CurrentUser;
                if (user == null) return;

                // FA-14: dieselbe Sichtbarkeit wie in der Aufgabenliste (eigene Klassen + selbst erstellte Aufgaben)
                _alltasks = await _taskService.GetVisibleTasksForUserAsync(user);

                BuildCalendar();
                BuildUpcomingTasks();

                OnPropertyChanged(nameof(MonthTitle));
                OnPropertyChanged(nameof(TodayTaskCount));
                OnPropertyChanged(nameof(WeekTaskCount));
                OnPropertyChanged(nameof(overdueTaskCount));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden des Kalenders: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviousMonth()
        {
            _currentMonth = _currentMonth.AddMonths(-1);

            BuildCalendar();
            BuildUpcomingTasks();

            OnPropertyChanged(nameof(MonthTitle));
        }

        private void NextMonth()
        {
            _currentMonth = _currentMonth.AddMonths(1);
            BuildCalendar();
            BuildUpcomingTasks();
            OnPropertyChanged(nameof(MonthTitle));
        }

        private void GoToToday()
        {
            _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            BuildCalendar();
            BuildUpcomingTasks();
            OnPropertyChanged(nameof(MonthTitle));
        }

        private void BuildCalendar()
        {
            CalendarDays.Clear();

            DateTime firstDateofMonth = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int mondayBasedDay = ((int)firstDateofMonth.DayOfWeek + 6 ) % 7;

            DateTime calendarStart = firstDateofMonth.AddDays(-mondayBasedDay);

            for (int i = 0; i < 42; i++)
            {
                DateTime date = calendarStart.AddDays(i);

                var day = new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = date.Month == _currentMonth.Month
                };

                var tasksForDay = _alltasks
                    .Where(task => task.Deadline.Date == date.Date)
                    .OrderBy(task => task.Status)
                    .ThenBy(task => task.Priority)
                    .ToList();

                foreach (var task in tasksForDay)
                {
                    day.Tasks.Add(task);
                }

                CalendarDays.Add(day);
            }
        }

        private void BuildUpcomingTasks()
        {
            UpcomingTasks.Clear();

            var tasks = _alltasks
               .Where(task => task.Status != 2)
               .OrderBy(task => task.Deadline)
               .Take(8)
               .ToList();

            foreach (var task in tasks)
            {
                UpcomingTasks.Add(task);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
