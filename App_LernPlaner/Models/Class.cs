using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class Class
    {
        public int ClassId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string JoinCode { get; set; } = string.Empty; // zufällig generierter code
        public DateTime CreatedAt { get; set; }
        public List<ClassMember> Members { get; set; } = new();
        public List<StudyTask> Tasks { get; set; } = new();
    }
}
