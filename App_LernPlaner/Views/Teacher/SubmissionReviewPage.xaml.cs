using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Teacher
{
    public class SubmissionRowVm
    {
        public int SubmissionId { get; set; }
        public int TaskId { get; set; }
        public int StudentUserId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Abgabe { get; set; } = string.Empty;
        public string LateText { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
    }

    // FA-19/20/21, UC-11: Abgaben ansehen und bewerten
    public partial class SubmissionReviewPage : Page
    {
        private readonly int _taskId;
        private readonly TaskService _taskService = new();
        private readonly SubmissionService _submissionService = new();
        private readonly Repositories.UserRepository _users = new();

        public SubmissionReviewPage(int taskId)
        {
            InitializeComponent();
            _taskId = taskId;
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var task = await _taskService.GetByIdAsync(_taskId);
            if (task == null) return;

            LblTaskTitle.Text = task.Titel;

            var submissions = await _submissionService.GetSubmissionsForTaskAsync(_taskId);
            var users = await _users.GetAllAsync();

            SubmissionsList.ItemsSource = submissions.Select(s => new SubmissionRowVm
            {
                SubmissionId = s.SubmissionId,
                TaskId = s.TaskId,
                StudentUserId = s.UserId,
                StudentName = users.FirstOrDefault(u => u.UserId == s.UserId)?.UserName ?? $"User {s.UserId}",
                Abgabe = string.IsNullOrWhiteSpace(s.Abgabe) ? "(kein Text)" : s.Abgabe,
                LateText = s.SubmittedAt > task.Deadline ? "verspätet" : "pünktlich",
                StatusText = s.IsGraded ? $"Bewertet (Note {s.Grade})" : "Noch nicht bewertet",
                Grade = s.Grade ?? string.Empty,
                Feedback = s.Feedback ?? string.Empty
            }).ToList();
        }

        private async void BtnSaveGrade_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int submissionId) return;
            if (btn.Parent is not Grid row) return;

            var gradeBox = row.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "TxtGrade");
            var feedbackBox = row.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "TxtFeedback");
            if (gradeBox == null) return;

            if (SubmissionsList.ItemsSource is not IEnumerable<SubmissionRowVm> rows) return;
            var vm = rows.FirstOrDefault(r => r.SubmissionId == submissionId);
            if (vm == null) return;

            var (success, error) = await _submissionService.GradeAsync(submissionId, vm.TaskId, vm.StudentUserId, gradeBox.Text.Trim(), feedbackBox?.Text.Trim() ?? string.Empty);
            if (!success)
            {
                MessageBox.Show(error, "Bewertung fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await LoadAsync();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => AppNavigator.NavigateTo(new SubmissionReviewTaskListPage());
    }
}
