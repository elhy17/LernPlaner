using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Teacher
{
    // FA-12/FA-13: Aufgabe erstellen/bearbeiten/löschen
    public partial class TaskEditorPage : Page
    {
        private readonly TaskService _taskService = new();
        private readonly ClassService _classService = new();
        private readonly StudyTask? _editingTask;

        public TaskEditorPage(StudyTask? editingTask)
        {
            InitializeComponent();
            _editingTask = editingTask;

            if (editingTask != null)
            {
                LblHeader.Text = "Aufgabe bearbeiten";
                BtnDelete.Visibility = Visibility.Visible;
                TxtTitle.Text = editingTask.Titel;
                TxtSubject.Text = editingTask.Subject;
                TxtDescription.Text = editingTask.Description;
                DpDeadline.SelectedDate = editingTask.Deadline;
                foreach (ComboBoxItem item in CmbPriority.Items)
                    if (item.Tag?.ToString() == editingTask.Priority.ToString())
                        item.IsSelected = true;
            }
            else
            {
                DpDeadline.SelectedDate = DateTime.Today.AddDays(7);
            }

            Loaded += async (_, _) => await LoadClassesAsync();
        }

        private async System.Threading.Tasks.Task LoadClassesAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var classes = (await _classService.GetAllClassesAsync()).Where(c => user.IsAdmin || c.TeacherId == user.UserId).ToList();
            CmbClass.ItemsSource = classes;

            if (_editingTask?.ClassId is int classId)
                CmbClass.SelectedItem = classes.FirstOrDefault(c => c.ClassId == classId);
            else if (classes.Count > 0)
                CmbClass.SelectedIndex = 0;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            if (string.IsNullOrWhiteSpace(TxtTitle.Text) || string.IsNullOrWhiteSpace(TxtSubject.Text) || DpDeadline.SelectedDate == null)
            {
                MessageBox.Show("Bitte Titel, Fach und Deadline ausfüllen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            byte priority = 2;
            if (CmbPriority.SelectedItem is ComboBoxItem prioItem && byte.TryParse(prioItem.Tag?.ToString(), out var p)) priority = p;

            int? classId = (CmbClass.SelectedItem as Class)?.ClassId;

            if (_editingTask == null)
            {
                var task = new StudyTask
                {
                    UserId = user.UserId,
                    CreatedBy = user.UserId,
                    ClassId = classId,
                    Titel = TxtTitle.Text.Trim(),
                    Subject = TxtSubject.Text.Trim(),
                    Description = TxtDescription.Text.Trim(),
                    Deadline = DpDeadline.SelectedDate.Value,
                    Priority = priority,
                    Status = 0,
                    IsRecurring = false,
                    ButtonText = "Öffnen"
                };
                await _taskService.AddTaskAsync(task);
            }
            else
            {
                _editingTask.Titel = TxtTitle.Text.Trim();
                _editingTask.Subject = TxtSubject.Text.Trim();
                _editingTask.Description = TxtDescription.Text.Trim();
                _editingTask.Deadline = DpDeadline.SelectedDate.Value;
                _editingTask.Priority = priority;
                _editingTask.ClassId = classId;
                await _taskService.UpdateTaskAsync(_editingTask);
            }

            AppNavigator.NavigateTo(new TaskEditorListPage());
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingTask == null) return;

            var result = MessageBox.Show("Diese Aufgabe wirklich löschen? Alle Abgaben dazu werden mitgelöscht.", "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            await _taskService.DeleteTaskAsync(_editingTask.TaskId);
            AppNavigator.NavigateTo(new TaskEditorListPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => AppNavigator.NavigateTo(new TaskEditorListPage());
    }
}
