using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    internal class Users
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash {  get; set; }
        public string Email {  get; set; }
        public string Role { get; set; }
        public string ProfilePicture { get; set; }
        public int XPPoints { get; set; }
        public DateTime CreatedAt { get; set; }

     



    }
}
