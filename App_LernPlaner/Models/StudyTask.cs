using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class StudyTask
    {
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string ButtonText { get; set; } 
        public int? ClassId { get; set; }
        public int? CreatedBy { get; set; }
        public string Titel { get; set; }
        public string Subject { get; set; }
        public DateTime Deadline { get; set; }
        public byte Priority { get; set; } // Wenn 1= Niedrig, 2= Mittel, 3 = Hoch
        public byte Status { get; set; } // 0 = Offen, 1 = In Bearbeitung, 2 = Erledigt
        public bool IsRecurring { get; set; }

        public string PriorityName => Priority switch
        {
            1 => "Niedrig",
            2 => "Mittel",
            3 => "Hoch",
            _ => "Unbekannt"
        };

        public string StatusName => Status switch
        {
            0 => "Offen",
            1 => "In Bearbeitung",
            2 => "Erledigt",
            _ => "Unbekannt"
        };

        public bool IsOverdue => Deadline < DateTime.Now && Status != 2;

        public List<SubTask> SubTasks { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
    }
}
