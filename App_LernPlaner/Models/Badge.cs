using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class Badge
    {
        public int BadgeId { get; set; }
        public string Name { get; set; } // z.B : Fleißiger schüler
        public string Description { get; set; } // z.b : 5 aufgaben an einem tag erledigt
        public string IconPath { get; set; } 
        public int XPReward { get; set; } 

    }
}
