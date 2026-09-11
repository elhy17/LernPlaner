using System.Collections.Generic;
using System.Threading.Tasks;
using App_LernPlaner.Models;
using App_LernPlaner.Repositories;

namespace App_LernPlaner.Services
{
    /// <summary>
    /// Chat zwischen allen Benutzern (Schüler, Lehrer, Admin) - rein DB-basiert,
    /// UI aktualisiert per Timer/Polling statt WebSocket.
    /// </summary>
    public class MessageService
    {
        private readonly MessageRepository _messages = new();

        public Task<List<Message>> GetConversationAsync(int userA, int userB) => _messages.GetConversationAsync(userA, userB);

        public Task<List<Contact>> GetContactsAsync(int userId) => _messages.GetContactsWithLastMessageAsync(userId);

        public Task<int> CountUnreadAsync(int userId) => _messages.CountUnreadTotalAsync(userId);

        public Task<int> SendAsync(int senderId, int receiverId, string body) => _messages.SendAsync(senderId, receiverId, body);

        public async Task<bool> EditAsync(int messageId, int currentUserId, string newBody)
        {
            if (string.IsNullOrWhiteSpace(newBody)) return false;
            await _messages.EditAsync(messageId, currentUserId, newBody);
            return true;
        }

        public Task DeleteAsync(int messageId, int currentUserId) => _messages.SoftDeleteAsync(messageId, currentUserId);

        public Task MarkConversationReadAsync(int userId, int otherUserId) => _messages.MarkConversationReadAsync(userId, otherUserId);

        // Admin-Dashboard "Anfragen": alle an den Admin gerichteten Nachrichten.
        public Task<List<Message>> GetInboxAsync(int adminUserId) => _messages.GetInboxAsync(adminUserId);
    }
}
