using App_LernPlaner.Models;

namespace App_LernPlaner.Services
{
    /// <summary>Hält den aktuell eingeloggten Benutzer für die Dauer der Anwendungslaufzeit.</summary>
    public static class sessionManager
    {
        public static User? CurrentUser { get; set; }

        public static int CurrentUserId => CurrentUser?.UserId ?? 0;

        public static bool IsLoggedIn => CurrentUser != null;

        public static async System.Threading.Tasks.Task LogoutAsync()
        {
            try
            {
                if (Remote.RemoteClient.Current is { } remote) await remote.LogoutAsync();
            }
            catch (System.Exception ex) when (ex is System.InvalidOperationException or System.TimeoutException or System.UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine("Remote logout unavailable; local session cleared.");
            }
            finally { Remote.RemoteClient.Current?.ClearSession(); CurrentUser = null; }
        }
    }
}
