using System.Threading.Tasks;

namespace App_LernPlaner.Services
{
    /// <summary>
    /// Kein SMTP konfiguriert (bewusste Entscheidung, siehe Pflichtenheft FA-04).
    /// RecoveryPage zeigt das temporäre Passwort stattdessen direkt auf dem Bildschirm an.
    /// </summary>
    public class EmailService
    {
        public bool IsConfigured => false;

        public Task<bool> TrySendPasswordResetEmailAsync(string toEmail, string tempPassword)
        {
            return Task.FromResult(false);
        }
    }
}
