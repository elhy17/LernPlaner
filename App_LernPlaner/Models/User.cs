using System;

namespace App_LernPlaner.Models
{
    public static class Roles
    {
        public const string Schueler = "Schüler";
        public const string Lehrer = "Lehrer";
        // In der Datenbank (Users.Role) steht für Administratoren "IT Admin".
        public const string Admin = "IT Admin";
    }

    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = Roles.Schueler;
        public string ProfilePicture { get; set; } = "default.png";
        public int XPPoints { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ClassName { get; set; }

        public bool IsTeacher => Role == Roles.Lehrer;
        public bool IsStudent => Role == Roles.Schueler;
        public bool IsAdmin => Role == Roles.Admin;

        public int Level => (XPPoints / 200) + 1;

        public string FullName =>
            !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName)
                ? $"{FirstName} {LastName}".Trim()
                : UserName;

        public string Initials
        {
            get
            {
                string first = string.IsNullOrWhiteSpace(FirstName) ? "" : FirstName!.Substring(0, 1).ToUpper();
                string last = string.IsNullOrWhiteSpace(LastName) ? "" : LastName!.Substring(0, 1).ToUpper();
                string combo = first + last;
                if (!string.IsNullOrWhiteSpace(combo)) return combo;
                return UserName.Length >= 2 ? UserName.Substring(0, 2).ToUpper() : "U";
            }
        }
    }
}
