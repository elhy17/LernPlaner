using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Admin
{
    public class AdminClassRowVm
    {
        public string Name { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int TaskCount { get; set; }
    }

    // Admin: systemweite Klassenübersicht (read-only, 3.2.1 "Zugriff auf alle Systemstatistiken")
    public partial class AdminClassesPage : Page
    {
        private readonly ClassService _classService = new();
        private readonly TaskService _taskService = new();

        public AdminClassesPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var classes = await _classService.GetAllClassesAsync();
            var allTasks = await _taskService.GetTasksAsync();

            var rows = new List<AdminClassRowVm>();
            foreach (var c in classes)
            {
                var teacher = (await _classService.GetMembersAsync(c.ClassId)).FirstOrDefault(m => m.UserId == c.TeacherId);
                rows.Add(new AdminClassRowVm
                {
                    Name = c.Name,
                    TeacherName = teacher?.UserName ?? "-",
                    StudentCount = await _classService.CountStudentsAsync(c.ClassId),
                    TaskCount = allTasks.Count(t => t.ClassId == c.ClassId)
                });
            }

            ClassesList.ItemsSource = rows;
        }
    }
}
