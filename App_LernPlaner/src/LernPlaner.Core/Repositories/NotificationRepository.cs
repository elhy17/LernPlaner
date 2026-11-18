using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core.Repositories;
public sealed class NotificationRepository(LernPlanerDbContext db)
{
    public async Task<List<Notification>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.Notifications.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.TriggerAt).ToListAsync(cancellationToken);
    }

    public async Task<int> CountUnreadAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead && x.Message != "__daily_login__", cancellationToken);
    }

    public async Task InsertAsync(int userId, string message, CancellationToken cancellationToken = default)
    {
        Input.Text(message, 2000);
        db.Notifications.Add(new Notification { UserId = userId, Message = message, TriggerAt = DateTime.Now, IsRead = message == "__daily_login__" });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsSimilarAsync(int userId, string messagePrefix, CancellationToken cancellationToken = default)
    {
        return await db.Notifications.AnyAsync(x => x.UserId == userId && x.Message.StartsWith(messagePrefix), cancellationToken);
    }

    public async Task MarkAsReadAsync(int notifId, CancellationToken cancellationToken = default)
    {
        await db.Notifications.Where(x => x.NotifId == notifId).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true), cancellationToken);
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        await db.Notifications.Where(x => x.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true), cancellationToken);
    }
}
