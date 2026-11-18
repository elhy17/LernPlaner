using System;
using System.Linq;
using System.Threading.Tasks;
using App_LernPlaner.Data;
using App_LernPlaner.Models;
using App_LernPlaner.Repositories;

namespace App_LernPlaner.Services
{
    public record AuthResult(bool Success, User? User, string Error);

    /// <summary>
    /// Login / Registrierung / Passwort-Reset (FA-01 bis FA-04).
    /// Arbeitet je nach AppConfig.UseMockData entweder auf den In-Memory
    /// Mock-Daten oder über EF Core oder die HTTPS API.
    /// </summary>
    public class AuthService
    {
        private readonly UserRepository _users = new();

        public async Task<AuthResult> TryLoginAsync(string username, string password)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current is { } remote)
            {
                var login = await remote.LoginAsync(username, password);
                return login == null ? new AuthResult(false, null, "Anmeldung fehlgeschlagen.") : new AuthResult(true, login.User, "");
            }
            username = (username ?? string.Empty).Trim();

            User? found = AppConfig.UseMockData
                ? MockDatabase.Users.FirstOrDefault(u => string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase))
                : await _users.FindByUsernameAsync(username);

            if (found == null)
                return new AuthResult(false, null, "Benutzer nicht gefunden.");

            if (!BCrypt.Net.BCrypt.Verify(password, found.PasswordHash))
                return new AuthResult(false, null, "Falsches Passwort.");

            if (!found.IsActive)
                return new AuthResult(false, null, "Dieses Konto wurde vom Administrator deaktiviert.");

            return new AuthResult(true, found, string.Empty);
        }

        /// <summary>
        /// Benutzer anlegen. Nur Admin/Lehrer/Schüler dürfen so erstellt werden - "IT Admin"
        /// wird hier serverseitig abgelehnt, Admin-Rechte werden ausschließlich über
        /// UserRepository.UpdateRoleAsync (Admin-Dashboard "Rolle wechseln") vergeben.
        /// Registrierung ist nicht öffentlich: dieser Pfad wird nur vom Admin-Dashboard aufgerufen.
        /// </summary>
        public async Task<AuthResult> TryRegisterAsync(string username, string password, string email, string role)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current is { } remote)
            {
                var user = await remote.PostAsync<User>("auth/register", new LernPlaner.Core.RegisterRequest(username,password,email,role));
                return new AuthResult(true,user,"");
            }
            LernPlaner.Core.Input.Password(password);

            username = (username ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();

            if (role != Roles.Schueler && role != Roles.Lehrer)
                return new AuthResult(false, null, "Ungültige Rolle. Nur Schüler oder Lehrer können hier angelegt werden.");

            bool exists = AppConfig.UseMockData
                ? MockDatabase.Users.Any(u =>
                    string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase))
                : await _users.ExistsAsync(username, email);

            if (exists)
                return new AuthResult(false, null, "Ein Konto mit diesem Benutzernamen oder dieser E-Mail existiert bereits.");

            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            if (AppConfig.UseMockData)
            {
                var newUser = new User
                {
                    UserId = MockDatabase.NextUserId(),
                    UserName = username,
                    Email = email,
                    PasswordHash = hash,
                    Role = role,
                    CreatedAt = DateTime.Now
                };
                MockDatabase.Users.Add(newUser);
                return new AuthResult(true, newUser, string.Empty);
            }

            int newId = await _users.InsertAsync(username, hash, email, role);
            var created = await _users.GetByIdAsync(newId);
            return new AuthResult(true, created, string.Empty);
        }

        /// <summary>
        /// Passwort zurücksetzen (FA-04). Erzeugt ein neues temporäres Passwort,
        /// speichert dessen Hash in der Datenbank und liefert es zur Anzeige/zum
        /// Versand per E-Mail zurück (siehe EmailService).
        /// </summary>
        public async Task<(bool Success, User? User, string TempPasswordOrError)> TryResetPasswordAsync(string email)
        {
            // The legacy unauthenticated reset displayed a new password to anyone knowing an email.
            // A verified email delivery/token flow is not configured, so fail closed in real modes.
            if (!AppConfig.UseMockData) return (false,null,"Bitte den Administrator um eine sichere Passwort-Zurücksetzung bitten.");

            email = (email ?? string.Empty).Trim();

            User? found = AppConfig.UseMockData
                ? MockDatabase.Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase))
                : await _users.FindByEmailAsync(email);

            if (found == null)
                return (false, null, "Kein Konto mit dieser E-Mail-Adresse gefunden.");

            string tempPassword = Guid.NewGuid().ToString("N").Substring(0, 10);
            string newHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            if (AppConfig.UseMockData)
                found.PasswordHash = newHash;
            else
                await _users.UpdatePasswordAsync(found.UserId, newHash);

            return (true, found, tempPassword);
        }

        /// <summary>
        /// Eigenes Passwort ändern (FA-06), während man eingeloggt ist - im Unterschied zu
        /// TryResetPasswordAsync muss hier das aktuelle Passwort korrekt eingegeben werden.
        /// </summary>
        public async Task<(bool Success, string Error)> TryChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current is { } remote)
            {
                await remote.PostAsync<bool>("auth/password",new LernPlaner.Core.PasswordRequest(userId,currentPassword,newPassword));
                // All old tokens were revoked. Renew this client session using the new password.
                var renewed = await remote.LoginAsync(sessionManager.CurrentUser?.UserName ?? "",newPassword);
                if (renewed == null) return (false,"Bitte erneut anmelden.");
                sessionManager.CurrentUser = renewed.User;
                return (true,"");
            }
            LernPlaner.Core.Input.Password(newPassword);

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                return (false, "Das neue Passwort muss mindestens 8 Zeichen lang sein.");

            User? user = AppConfig.UseMockData
                ? MockDatabase.Users.FirstOrDefault(u => u.UserId == userId)
                : await _users.GetByIdAsync(userId);

            if (user == null)
                return (false, "Benutzer nicht gefunden.");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return (false, "Das aktuelle Passwort ist falsch.");

            string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            if (AppConfig.UseMockData)
                user.PasswordHash = newHash;
            else
                await _users.UpdatePasswordAsync(userId, newHash);

            return (true, string.Empty);
        }
    }
}
