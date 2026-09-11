using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Teacher
{
    public class GradingTaskRowVm
    {
        public int TaskId { get; set; }
        public string Titel { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int SubmittedCount { get; set; }
        public int GradedCount { get; set; }
    }

    // FA-19: Lehrer sehen alle Abgaben ihrer Aufgaben
    public partial class SubmissionReviewTaskListPage : Page
    {
        private readonly TaskService _taskService = new();
        private readonly ClassService _classService = new();
        private readonly SubmissionService _submissionService = new();

        public SubmissionReviewTaskListPage()
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

            var allTasks = (await _taskService.GetTasksAsync()).Where(t => t.ClassId.HasValue && classIds.Contains(t.ClassId.Value));

            var rows = new List<GradingTaskRowVm>();
            foreach (var t in allTasks)
            {
                var submissions = await _submissionService.GetSubmissionsForTaskAsync(t.TaskId);
                rows.Add(new GradingTaskRowVm
                {
                    TaskId = t.TaskId,
                    Titel = t.Titel,
                    ClassName = myClasses.First(c => c.ClassId == t.ClassId).Name,
                    SubmittedCount = submissions.Count,
                    GradedCount = submissions.Count(s => s.IsGraded)
                });
            }

            TasksList.ItemsSource = rows;
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int taskId) return;
            AppNavigator.NavigateTo(new SubmissionReviewPage(taskId));
        }
    }
}
