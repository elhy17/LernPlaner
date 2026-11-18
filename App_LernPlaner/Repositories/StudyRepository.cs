using App_LernPlaner.Models;
using LernPlaner.Core;
using App_LernPlaner.Remote;

namespace App_LernPlaner.Repositories;
public class StudyRepository
{
    public async Task LogSessionAsync(int userId, int taskId, int durationMin, string notes, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Study.LogSessionAsync", new object? [] { userId, taskId, durationMin, notes }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.StudyRepository(db).LogSessionAsync(userId, taskId, durationMin, notes, cancellationToken);
    }

    public async Task<List<StudySession>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<StudySession>>("Study.GetForUserAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.StudyRepository(db).GetForUserAsync(userId, cancellationToken);
    }

    public async Task<int> GetTotalMinutesAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<int>("Study.GetTotalMinutesAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.StudyRepository(db).GetTotalMinutesAsync(userId, cancellationToken);
    }
}
