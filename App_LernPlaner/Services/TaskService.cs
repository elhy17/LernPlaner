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
        public async Task<List<Models.StudyTask>> GetTasksAsync()
        {
            return await _taskRepository.GetAllTasks();
        }

        public async Task<List<Models.StudyTask>> GetTasksByUserIdAsync(int userId)
        {
            return await _taskRepository.GetTasksByUserIdAsync(userId);
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