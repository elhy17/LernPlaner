using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class ClassMember
    {
        public int ClassId { get; set; }
        public int UserId { get; set; }
        public byte Role { get; set; } // 1 = Lehrer, 2 = Schüler
        public DateTime JoinedAt { get; set; }

    }
}
