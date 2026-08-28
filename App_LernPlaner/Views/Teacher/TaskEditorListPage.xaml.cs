using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Teacher
{
    public class TeacherTaskRowVm
    {
        public int TaskId { get; set; }
        public string Titel { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public System.DateTime Deadline { get; set; }
    }

    // FA-12/FA-13: Aufgaben erstellen/bearbeiten/löschen (Lehrer-Sicht)
    public partial class TaskEditorListPage : Page
    {
        private readonly TaskService _taskService = new();
        private readonly ClassService _classService = new();

        public TaskEditorListPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var myClasses = (await _classService.GetAllClassesAsync()).Where(c => user.IsAdmin || c.TeacherId == user.UserId).ToList();
            var classIds = myClasses.Select(c => c.ClassId).ToHashSet();

            var allTasks = await _taskService.GetTasksAsync();
            var rows = allTasks
                .Where(t => t.ClassId.HasValue && classIds.Contains(t.ClassId.Value))
                .OrderBy(t => t.Deadline)
                .Select(t => new TeacherTaskRowVm
                {
                    TaskId = t.TaskId,
                    Titel = t.Titel,
                    Subject = t.Subject,
                    ClassName = myClasses.First(c => c.ClassId == t.ClassId).Name,
                    Deadline = t.Deadline
                }).ToList();

            TasksList.ItemsSource = rows;
        }

        private void BtnNewTask_Click(object sender, RoutedEventArgs e) => AppNavigator.NavigateTo(new TaskEditorPage(null));

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int taskId) return;
            var task = await _taskService.GetByIdAsync(taskId);
            if (task != null) AppNavigator.NavigateTo(new TaskEditorPage(task));
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int taskId) return;

            var result = MessageBox.Show("Diese Aufgabe wirklich löschen? Alle Abgaben dazu werden mitgelöscht.", "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            await _taskService.DeleteTaskAsync(taskId);
            await LoadAsync();
        }
    }
}
