using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
   public class CalendarDay
    {
        public DateTime Date { get; set; }
        public string DayNumber => Date.Day.ToString();
        public bool IsCurrentMonth { get; set; }
        public bool IsToday => Date.Date == DateTime.Today;

        public ObservableCollection<StudyTask> Tasks { get; set; } = new();


        public string DayForeground =>
            IsCurrentMonth ? "White" : "#64748B";

        public string DayBackground =>
            IsToday ? "#1D4ED8" : "Transparent";

        public string DayBorder =>
            IsToday ? "#60A5FA" : "#243044"; 
    }
}
