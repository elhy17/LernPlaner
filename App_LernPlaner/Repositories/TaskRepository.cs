using App_LernPlaner.Models;
using LernPlaner.Core;
using App_LernPlaner.Remote;

namespace App_LernPlaner.Repositories;
public class TaskRepository
{
    public async Task<List<StudyTask>> GetAllTasks(CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<StudyTask>>("Task.GetAllTasks", new object? [] { }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.TaskRepository(db).GetAllTasks(cancellationToken);
    }

    public async Task<List<StudyTask>> GetTasksByUserIdAsync(int UserId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<StudyTask>>("Task.GetTasksByUserIdAsync", new object? [] { UserId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.TaskRepository(db).GetTasksByUserIdAsync(UserId, cancellationToken);
    }

    public async Task<StudyTask?> GetByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<StudyTask?>("Task.GetByIdAsync", new object? [] { taskId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.TaskRepository(db).GetByIdAsync(taskId, cancellationToken);
    }

    public async Task<List<StudyTask>> GetVisibleTasksForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<StudyTask>>("Task.GetVisibleTasksForUserAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.TaskRepository(db).GetVisibleTasksForUserAsync(userId, cancellationToken);
    }

    public async Task<List<StudyTask>> GetTasksByClassIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<StudyTask>>("Task.GetTasksByClassIdAsync", new object? [] { classId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.TaskRepository(db).GetTasksByClassIdAsync(classId, cancellationToken);
    }

    public async Task UpdateAsync(StudyTask task, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Task.UpdateAsync", new object? [] { task }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.TaskRepository(db).UpdateAsync(task, cancellationToken);
    }

    public async Task HinzufugenTask(StudyTask task, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Task.HinzufugenTask", new object? [] { task }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.TaskRepository(db).HinzufugenTask(task, cancellationToken);
    }

    public async Task StatusAendernAsync(int taskId, byte neuerStatus, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Task.StatusAendernAsync", new object? [] { taskId, neuerStatus }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.TaskRepository(db).StatusAendernAsync(taskId, neuerStatus, cancellationToken);
    }

    public async Task LoeschenAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Task.LoeschenAsync", new object? [] { taskId }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.TaskRepository(db).LoeschenAsync(taskId, cancellationToken);
    }
}
