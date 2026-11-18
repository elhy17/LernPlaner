using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class SubTask
    {
        public int SubTaskId { get; set; }
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int SortOrder { get; set; }
    }
}
