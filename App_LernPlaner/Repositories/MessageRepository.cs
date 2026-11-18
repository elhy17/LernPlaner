using App_LernPlaner.Models;
using LernPlaner.Core;
using App_LernPlaner.Remote;

namespace App_LernPlaner.Repositories;
public class MessageRepository
{
    public async Task<List<Message>> GetConversationAsync(int userA, int userB, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Message>>("Message.GetConversationAsync", new object? [] { userA, userB }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.MessageRepository(db).GetConversationAsync(userA, userB, cancellationToken);
    }

    public async Task<List<Contact>> GetContactsWithLastMessageAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Contact>>("Message.GetContactsWithLastMessageAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.MessageRepository(db).GetContactsWithLastMessageAsync(userId, cancellationToken);
    }

    public async Task<int> CountUnreadTotalAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<int>("Message.CountUnreadTotalAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.MessageRepository(db).CountUnreadTotalAsync(userId, cancellationToken);
    }

    public async Task<int> SendAsync(int senderId, int receiverId, string body, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<int>("Message.SendAsync", new object? [] { senderId, receiverId, body }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.MessageRepository(db).SendAsync(senderId, receiverId, body, cancellationToken);
    }

    public async Task EditAsync(int messageId, int senderId, string newBody, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Message.EditAsync", new object? [] { messageId, senderId, newBody }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.MessageRepository(db).EditAsync(messageId, senderId, newBody, cancellationToken);
    }

    public async Task SoftDeleteAsync(int messageId, int senderId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Message.SoftDeleteAsync", new object? [] { messageId, senderId }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.MessageRepository(db).SoftDeleteAsync(messageId, senderId, cancellationToken);
    }

    public async Task MarkConversationReadAsync(int userId, int otherUserId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Message.MarkConversationReadAsync", new object? [] { userId, otherUserId }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.MessageRepository(db).MarkConversationReadAsync(userId, otherUserId, cancellationToken);
    }

    public async Task<List<Message>> GetInboxAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Message>>("Message.GetInboxAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.MessageRepository(db).GetInboxAsync(userId, cancellationToken);
    }
}
