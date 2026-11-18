using App_LernPlaner.Models;
using LernPlaner.Core;
using App_LernPlaner.Remote;

namespace App_LernPlaner.Repositories;
public class SubmissionRepository
{
    public async Task SaveProgressAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Submission.SaveProgressAsync", new object? [] { submission }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.SubmissionRepository(db).SaveProgressAsync(submission, cancellationToken);
    }

    public async Task<Submission?> GetByTaskAndUserAsync(int taskId, int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<Submission?>("Submission.GetByTaskAndUserAsync", new object? [] { taskId, userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.SubmissionRepository(db).GetByTaskAndUserAsync(taskId, userId, cancellationToken);
    }

    public async Task<List<Submission>> GetForTaskAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Submission>>("Submission.GetForTaskAsync", new object? [] { taskId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.SubmissionRepository(db).GetForTaskAsync(taskId, cancellationToken);
    }

    public async Task<List<Submission>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Submission>>("Submission.GetForUserAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.SubmissionRepository(db).GetForUserAsync(userId, cancellationToken);
    }

    public async Task GradeAsync(int submissionId, string grade, string feedback, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Submission.GradeAsync", new object? [] { submissionId, grade, feedback }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.SubmissionRepository(db).GradeAsync(submissionId, grade, feedback, cancellationToken);
    }
}
