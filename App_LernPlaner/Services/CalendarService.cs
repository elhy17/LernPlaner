using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App_LernPlaner.Models;

namespace App_LernPlaner.Services
{
    /// <summary>Anstehende Deadlines für den Kalender (FA-23 Grundlage).</summary>
    public class CalendarService
    {
        private readonly TaskService _taskService = new();

        public async Task<List<StudyTask>> GetUpcomingDeadlinesAsync(User user)
        {
            var tasks = await _taskService.GetVisibleTasksForUserAsync(user);
            return tasks
                .Where(t => t.Status != 2 && t.Deadline >= DateTime.Today)
                .OrderBy(t => t.Deadline)
                .ToList();
        }
    }
}
