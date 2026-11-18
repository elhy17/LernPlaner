using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class DashboardStets
    {
        public int OffeneAufgaben { get; set; }
        public int Abgegeben { get; set; }
        public string Durchschnittsnote { get; set; } = string.Empty;
        public int XpGesamt { get; set; }
    }
}
