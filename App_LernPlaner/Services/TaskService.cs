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

        public TaskService()
        {
            _taskRepository = new Repositories.TaskRepository();
        }

        public async Task<List<Models.Task>> GetTasksAsync()
        {
            return await _taskRepository.GetAllTasks();
        }

        public async Task<int> GetOpenTaskCountAsync()
        {
            var tasks = await _taskRepository.GetAllTasks();
            return tasks.Count(x => x.Status == 0);
        }

        public async Task<int> GetFinishedTaskCountAsync()
        {
            var tasks = await _taskRepository.GetAllTasks();
            return tasks.Count(x => x.Status == 2);
        }
    }
}