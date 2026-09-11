using App_LernPlaner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Services
{
    public class TaskService
    {
        private readonly Repositories.TaskRepository _taskRepository;

        public TaskService(Repositories.TaskRepository  taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public TaskService()
        {
            _taskRepository = new Repositories.TaskRepository();
        }

        public async Task AddTaskAsync(Models.StudyTask task)
        {
            await _taskRepository.HinzufugenTask(task);
            await NotifyClassOnCreateAsync(task);
        }

        // FA-22: Benachrichtigung bei neuer Aufgabe für alle Mitglieder der Klasse
        private static async Task NotifyClassOnCreateAsync(Models.StudyTask task)
        {
            if (!task.ClassId.HasValue) return;

            var members = await new ClassService().GetMembersAsync(task.ClassId.Value, role: 2);
            var notifications = new NotificationService();
            foreach (var member in members)
                await notifications.NotifyAsync(member.UserId, $"Neue Aufgabe: {task.Titel}");
        }

        public async Task<List<Models.StudyTask>> GetTasksAsync()
        {
            return await _taskRepository.GetAllTasks();
        }

        public async Task<List<Models.StudyTask>> GetTasksByUserIdAsync(int userId)
        {
            return await _taskRepository.GetTasksByUserIdAsync(userId);
        }

        public async Task<Models.StudyTask?> GetByIdAsync(int taskId)
        {
            return await _taskRepository.GetByIdAsync(taskId);
        }

        // FA-14: für Klassenmitglieder und den Ersteller sichtbare Aufgaben
        public async Task<List<Models.StudyTask>> GetVisibleTasksForUserAsync(Models.User user)
        {
            return await _taskRepository.GetVisibleTasksForUserAsync(user.UserId);
        }

        public async Task<List<Models.StudyTask>> GetTasksByClassIdAsync(int classId)
        {
            return await _taskRepository.GetTasksByClassIdAsync(classId);
        }

        public async Task UpdateTaskAsync(Models.StudyTask task)
        {
            await _taskRepository.UpdateAsync(task);
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            await _taskRepository.LoeschenAsync(taskId);
        }

        // FA-15: Status einer Aufgabe aus Sicht eines bestimmten Schülers (0 Offen/1 In Bearbeitung/2 Abgegeben/3 Bewertet)
        public async Task<byte> GetStatusForStudentAsync(Models.StudyTask task, int userId)
        {
            var submission = await new Repositories.SubmissionRepository().GetByTaskAndUserAsync(task.TaskId, userId);
            if (submission == null) return task.Status;
            if (submission.IsGraded) return 3;
            return submission.Status;
        }

        public int CountOpenTasks(List<StudyTask> tasks)
        {
            return tasks.Count(x => x.Status == 0);
        }

        public int CountInProgressTasks(List<StudyTask> tasks)
        {
            return tasks.Count(x => x.Status == 1);
        }

        public int CountFinishedTasks(List<StudyTask> tasks)
        {
            return tasks.Count(x => x.Status == 2);
        }

        public int CountDueThisWeek(List<StudyTask> tasks)
        {
            var today = System.DateTime.Today;
            var nextweek = today.AddDays(7);

            return tasks.Count(x => x.Status != 2 
            && x.Deadline.Date >= today
            && x.Deadline.Date <= nextweek );
        }
    }
}