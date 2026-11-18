using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class UserBadge
    {
        public int UserId { get; set; }
        public int BadgeId { get; set; }
        public DateTime DateEarned { get; set; }

        // Navigation properties
        public Badge Badge { get; set; } = null!;
    }
}
