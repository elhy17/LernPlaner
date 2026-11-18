using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class Notification
    {
        public int NotifId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TriggerAt { get; set; }

        public bool IsRead { get; set; }

        public string TimeAgo
        {
            get
            {
                var diff = DateTime.Now - TriggerAt;
                if (diff.TotalMinutes < 60)
                    return $"Vor {(int)diff.TotalMinutes} Minuten ago";
                if (diff.TotalHours < 24)
                    return $"Vor {(int)diff.TotalHours} Stunden ago";
                return $"Vor {(int)diff.TotalDays} Tagen ago";
            }
        }
    }
}
