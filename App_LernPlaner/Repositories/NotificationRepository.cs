using App_LernPlaner.Models;
using LernPlaner.Core;
using App_LernPlaner.Remote;

namespace App_LernPlaner.Repositories;
public class NotificationRepository
{
    public async Task<List<Notification>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Notification>>("Notification.GetForUserAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.NotificationRepository(db).GetForUserAsync(userId, cancellationToken);
    }

    public async Task<int> CountUnreadAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<int>("Notification.CountUnreadAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.NotificationRepository(db).CountUnreadAsync(userId, cancellationToken);
    }

    public async Task InsertAsync(int userId, string message, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Notification.InsertAsync", new object? [] { userId, message }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.NotificationRepository(db).InsertAsync(userId, message, cancellationToken);
    }

    public async Task<bool> ExistsSimilarAsync(int userId, string messagePrefix, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<bool>("Notification.ExistsSimilarAsync", new object? [] { userId, messagePrefix }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.NotificationRepository(db).ExistsSimilarAsync(userId, messagePrefix, cancellationToken);
    }

    public async Task MarkAsReadAsync(int notifId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Notification.MarkAsReadAsync", new object? [] { notifId }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.NotificationRepository(db).MarkAsReadAsync(notifId, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Notification.MarkAllAsReadAsync", new object? [] { userId }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.NotificationRepository(db).MarkAllAsReadAsync(userId, cancellationToken);
    }
}
