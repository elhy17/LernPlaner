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
    /// Mock-Daten oder direkt gegen die MySQL Datenbank.
    /// </summary>
    public class AuthService
    {
        private readonly UserRepository _users = new();

        public async Task<AuthResult> TryLoginAsync(string username, string password)
        {
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
    }
}
