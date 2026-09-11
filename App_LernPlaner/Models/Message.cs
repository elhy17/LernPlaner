using System;

namespace App_LernPlaner.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRead { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public string TimeText => SentAt.ToString("dd.MM.yyyy HH:mm");
    }

    public class Contact
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
