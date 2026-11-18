using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class UserProfile
    {
        public int UserId { get; set; }



        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;

        public string ProfilePicturePath { get; set; } = string.Empty;
        public int XPPoints { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string Initials
        {
            get
            {
                string first = string.IsNullOrWhiteSpace(FirstName) ? "" : FirstName.Substring(0, 1).ToUpper();
                string last = string.IsNullOrWhiteSpace(LastName) ? "" : LastName.Substring(0, 1).ToUpper();

                if (!string.IsNullOrWhiteSpace(first + last))
                {
                    return first + last;
                }

                return Username.Length >= 2 ? Username.Substring(0, 2).ToUpper() : "U";
            }
        }

        public int Level => (XPPoints / 250) + 1;

        public string RoleAndClass => string.IsNullOrWhiteSpace(ClassName) ? Role : $"{Role} - Klasse {ClassName}";
    }
}
