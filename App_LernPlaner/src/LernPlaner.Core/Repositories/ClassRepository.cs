using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core.Repositories;
public sealed class ClassRepository(LernPlanerDbContext db)
{
    public async Task<List<Class>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Classes.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<Class?> GetByIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        return await db.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.ClassId == classId, cancellationToken);
    }

    public async Task<Class?> GetByJoinCodeAsync(string joinCode, CancellationToken cancellationToken = default)
    {
        return await db.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.JoinCode == joinCode, cancellationToken);
    }

    public async Task<List<Class>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.Classes.AsNoTracking().Where(x => db.ClassMembers.Any(m => m.ClassId == x.ClassId && m.UserId == userId)).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetMembersAsync(int classId, byte? role = null, CancellationToken cancellationToken = default)
    {
        return await db.Users.AsNoTracking().Where(x => db.ClassMembers.Any(m => m.ClassId == classId && m.UserId == x.UserId && (role == null || m.Role == role))).ToListAsync(cancellationToken);
    }

    public async Task<int> CountStudentsAsync(int classId, CancellationToken cancellationToken = default)
    {
        return await db.ClassMembers.CountAsync(x => x.ClassId == classId && x.Role == 2, cancellationToken);
    }

    public async Task<Class> CreateAsync(string name, int teacherId, string joinCode, CancellationToken cancellationToken = default)
    {
        Input.Text(name, 200);
        Input.Text(joinCode, 32);
        var c = new Class
        {
            Name = name,
            TeacherId = teacherId,
            JoinCode = joinCode,
            CreatedAt = DateTime.Now
        };
        c.Members.Add(new ClassMember { UserId = teacherId, Role = 1, JoinedAt = DateTime.Now });
        db.Classes.Add(c);
        await db.SaveChangesAsync(cancellationToken);
        return new Class
        {
            ClassId = c.ClassId,
            Name = c.Name,
            TeacherId = c.TeacherId,
            JoinCode = c.JoinCode,
            CreatedAt = c.CreatedAt
        };
    }

    public async Task<bool> IsMemberAsync(int classId, int userId, CancellationToken cancellationToken = default)
    {
        return await db.ClassMembers.AnyAsync(x => x.ClassId == classId && x.UserId == userId, cancellationToken);
    }

    public async Task AddMemberAsync(int classId, int userId, byte role, CancellationToken cancellationToken = default)
    {
        if (role != 1 && role != 2)
            throw new ArgumentException("Invalid member role.");
        db.ClassMembers.Add(new ClassMember { ClassId = classId, UserId = userId, Role = role, JoinedAt = DateTime.Now });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(int classId, int userId, CancellationToken cancellationToken = default)
    {
        await db.ClassMembers.Where(x => x.ClassId == classId && x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteAsync(int classId, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Tasks.Where(x => x.ClassId == classId).ExecuteUpdateAsync(s => s.SetProperty(x => x.ClassId, (int? )null), cancellationToken);
        await db.Classes.Where(x => x.ClassId == classId).ExecuteDeleteAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}
