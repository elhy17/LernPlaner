using App_LernPlaner.Models;

namespace App_LernPlaner.Services
{
    /// <summary>Hält den aktuell eingeloggten Benutzer für die Dauer der Anwendungslaufzeit.</summary>
    public static class sessionManager
    {
        public static User? CurrentUser { get; set; }

        public static int CurrentUserId => CurrentUser?.UserId ?? 0;

        public static bool IsLoggedIn => CurrentUser != null;

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
