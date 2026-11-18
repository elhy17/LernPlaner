using App_LernPlaner.Models;
using LernPlaner.Core;
using App_LernPlaner.Remote;

namespace App_LernPlaner.Repositories;
public class GamificationRepository
{
    public async Task<List<Badge>> GetAllBadgesAsync(CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Badge>>("Gamification.GetAllBadgesAsync", new object? [] { }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.GamificationRepository(db).GetAllBadgesAsync(cancellationToken);
    }

    public async Task<List<UserBadge>> GetUserBadgesAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<UserBadge>>("Gamification.GetUserBadgesAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.GamificationRepository(db).GetUserBadgesAsync(userId, cancellationToken);
    }

    public async Task<bool> HasBadgeAsync(int userId, int badgeId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<bool>("Gamification.HasBadgeAsync", new object? [] { userId, badgeId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.GamificationRepository(db).HasBadgeAsync(userId, badgeId, cancellationToken);
    }

    public async Task<int?> GetBadgeIdByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<int?>("Gamification.GetBadgeIdByNameAsync", new object? [] { name }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.GamificationRepository(db).GetBadgeIdByNameAsync(name, cancellationToken);
    }

    public async Task AwardBadgeAsync(int userId, int badgeId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Gamification.AwardBadgeAsync", new object? [] { userId, badgeId }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.GamificationRepository(db).AwardBadgeAsync(userId, badgeId, cancellationToken);
    }

    public async Task<List<User>> GetLeaderboardAsync(int classId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<User>>("Gamification.GetLeaderboardAsync", new object? [] { classId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.GamificationRepository(db).GetLeaderboardAsync(classId, cancellationToken);
    }
}
