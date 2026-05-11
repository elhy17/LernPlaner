using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class StudySession
    {
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public uint TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMin { get; set; }
        public string Notes { get; set; }

        public string DurationText => 
            DurationMin >= 60 ? $"{DurationMin / 60}h {DurationMin % 60}m" 
            : $"{DurationMin}m";

    }
}
