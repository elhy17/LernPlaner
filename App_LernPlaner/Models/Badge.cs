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
        public string Name { get; set; } = string.Empty; // z.B : Fleißiger schüler
        public string Description { get; set; } = string.Empty; // z.b : 5 aufgaben an einem tag erledigt
        public string IconPath { get; set; } = string.Empty;
        public int XPReward { get; set; }

    }
}
