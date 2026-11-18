using App_LernPlaner.Models;
using LernPlaner.Core;
using App_LernPlaner.Remote;

namespace App_LernPlaner.Repositories;
public class ClassRepository
{
    public async Task<List<Class>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Class>>("Class.GetAllAsync", new object? [] { }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.ClassRepository(db).GetAllAsync(cancellationToken);
    }

    public async Task<Class?> GetByIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<Class?>("Class.GetByIdAsync", new object? [] { classId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.ClassRepository(db).GetByIdAsync(classId, cancellationToken);
    }

    public async Task<Class?> GetByJoinCodeAsync(string joinCode, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<Class?>("Class.GetByJoinCodeAsync", new object? [] { joinCode }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.ClassRepository(db).GetByJoinCodeAsync(joinCode, cancellationToken);
    }

    public async Task<List<Class>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<Class>>("Class.GetForUserAsync", new object? [] { userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.ClassRepository(db).GetForUserAsync(userId, cancellationToken);
    }

    public async Task<List<User>> GetMembersAsync(int classId, byte? role = null, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<List<User>>("Class.GetMembersAsync", new object? [] { classId, role }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.ClassRepository(db).GetMembersAsync(classId, role, cancellationToken);
    }

    public async Task<int> CountStudentsAsync(int classId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<int>("Class.CountStudentsAsync", new object? [] { classId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.ClassRepository(db).CountStudentsAsync(classId, cancellationToken);
    }

    public async Task<Class> CreateAsync(string name, int teacherId, string joinCode, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<Class>("Class.CreateAsync", new object? [] { name, teacherId, joinCode }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.ClassRepository(db).CreateAsync(name, teacherId, joinCode, cancellationToken);
    }

    public async Task<bool> IsMemberAsync(int classId, int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
            return await RemoteClient.Current.CallAsync<bool>("Class.IsMemberAsync", new object? [] { classId, userId }, cancellationToken);
        await using var db = DatabaseSettings.Create();
        return await new LernPlaner.Core.Repositories.ClassRepository(db).IsMemberAsync(classId, userId, cancellationToken);
    }

    public async Task AddMemberAsync(int classId, int userId, byte role, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Class.AddMemberAsync", new object? [] { classId, userId, role }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.ClassRepository(db).AddMemberAsync(classId, userId, role, cancellationToken);
    }

    public async Task RemoveMemberAsync(int classId, int userId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Class.RemoveMemberAsync", new object? [] { classId, userId }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.ClassRepository(db).RemoveMemberAsync(classId, userId, cancellationToken);
    }

    public async Task DeleteAsync(int classId, CancellationToken cancellationToken = default)
    {
        if (RemoteClient.Current != null)
        {
            await RemoteClient.Current.CallAsync("Class.DeleteAsync", new object? [] { classId }, cancellationToken);
            return;
        }

        await using var db = DatabaseSettings.Create();
        await new LernPlaner.Core.Repositories.ClassRepository(db).DeleteAsync(classId, cancellationToken);
    }
}
