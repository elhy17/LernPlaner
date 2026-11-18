using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core.Repositories;
public sealed class MessageRepository(LernPlanerDbContext db)
{
    private async Task<List<Message>> DecorateAsync(IQueryable<Message> query, CancellationToken ct)
    {
        var messages = await query.ToListAsync(ct);
        var ids = messages.Select(x => x.SenderId).ToList();
        var users = await db.Users.AsNoTracking().Where(x => ids.Contains(x.UserId)).ToDictionaryAsync(x => x.UserId, ct);
        foreach (var m in messages)
        {
            m.SenderName = users.TryGetValue(m.SenderId, out var u) ? u.UserName : "";
            if (m.IsDeleted)
                m.Body = "(Nachricht gelöscht)";
        }

        return messages;
    }

    public async Task<List<Message>> GetConversationAsync(int userA, int userB, CancellationToken cancellationToken = default)
    {
        return await DecorateAsync(db.Messages.AsNoTracking().Where(x => (x.SenderId == userA && x.ReceiverId == userB) || (x.SenderId == userB && x.ReceiverId == userA)).OrderBy(x => x.SentAt), cancellationToken);
    }

    public async Task<List<Contact>> GetContactsWithLastMessageAsync(int userId, CancellationToken cancellationToken = default)
    {
        var users = await db.Users.AsNoTracking().Where(x => x.UserId != userId && x.IsActive).ToListAsync(cancellationToken);
        var messages = await db.Messages.AsNoTracking().Where(x => x.SenderId == userId || x.ReceiverId == userId).OrderByDescending(x => x.SentAt).ToListAsync(cancellationToken);
        return users.Select(u =>
        {
            var last = messages.FirstOrDefault(x => x.SenderId == u.UserId || x.ReceiverId == u.UserId);
            return new Contact
            {
                UserId = u.UserId,
                DisplayName = u.FullName,
                Role = u.Role,
                LastMessage = last == null ? "" : last.IsDeleted ? "(Nachricht gelöscht)" : last.Body,
                LastMessageAt = last?.SentAt,
                UnreadCount = messages.Count(x => x.SenderId == u.UserId && x.ReceiverId == userId && !x.IsRead && !x.IsDeleted)
            };
        }).OrderByDescending(x => x.LastMessageAt).ToList();
    }

    public async Task<int> CountUnreadTotalAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.Messages.CountAsync(x => x.ReceiverId == userId && !x.IsRead && !x.IsDeleted, cancellationToken);
    }

    public async Task<int> SendAsync(int senderId, int receiverId, string body, CancellationToken cancellationToken = default)
    {
        Input.Text(body, 10000);
        var m = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Body = body,
            SentAt = DateTime.Now
        };
        db.Messages.Add(m);
        await db.SaveChangesAsync(cancellationToken);
        return m.MessageId;
    }

    public async Task EditAsync(int messageId, int senderId, string newBody, CancellationToken cancellationToken = default)
    {
        Input.Text(newBody, 10000);
        await db.Messages.Where(x => x.MessageId == messageId && x.SenderId == senderId && !x.IsDeleted).ExecuteUpdateAsync(s => s.SetProperty(x => x.Body, newBody).SetProperty(x => x.EditedAt, (DateTime? )DateTime.Now), cancellationToken);
    }

    public async Task SoftDeleteAsync(int messageId, int senderId, CancellationToken cancellationToken = default)
    {
        await db.Messages.Where(x => x.MessageId == messageId && x.SenderId == senderId).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true).SetProperty(x => x.Body, ""), cancellationToken);
    }

    public async Task MarkConversationReadAsync(int userId, int otherUserId, CancellationToken cancellationToken = default)
    {
        await db.Messages.Where(x => x.ReceiverId == userId && x.SenderId == otherUserId).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true), cancellationToken);
    }

    public async Task<List<Message>> GetInboxAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await DecorateAsync(db.Messages.AsNoTracking().Where(x => x.ReceiverId == userId && !x.IsDeleted).OrderByDescending(x => x.SentAt), cancellationToken);
    }
}
