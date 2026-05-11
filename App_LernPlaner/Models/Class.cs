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
        public string Name { get; set; }
        public int TeacherId { get; set; }
        public string JoinCode { get; set; } // zufällig generierter code
        public DateTime CreatedAt { get; set; }
        public List<ClassMember> Members { get; set; } = new();
        public List<Task> Tasks { get; set; } = new();
    }
}
