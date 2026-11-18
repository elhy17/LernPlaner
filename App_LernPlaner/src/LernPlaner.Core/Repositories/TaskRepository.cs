using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core.Repositories;
public sealed class TaskRepository(LernPlanerDbContext db)
{
    private async Task<List<StudyTask>> DecorateAsync(IQueryable<StudyTask> query, int? userId, CancellationToken ct)
    {
        var tasks = await query.ToListAsync(ct);
        var ids = tasks.Select(x => x.CreatedBy).ToList();
        var users = await db.Users.AsNoTracking().Where(x => ids.Contains(x.UserId)).ToDictionaryAsync(x => x.UserId, ct);
        var tids = tasks.Select(x => x.TaskId).ToList();
        var submissions = userId.HasValue ? await db.Submissions.AsNoTracking().Where(x => x.UserId == userId && tids.Contains(x.TaskId)).ToDictionaryAsync(x => x.TaskId, ct) : new Dictionary<int, Submission>();
        foreach (var t in tasks)
        {
            t.ButtonText = "Öffnen";
            t.CreatedByUsername = t.CreatedBy.HasValue && users.TryGetValue(t.CreatedBy.Value, out var u) ? u.FullName : "Unbekannt";
            if (submissions.TryGetValue(t.TaskId, out var s))
                t.Status = s.IsGraded ? (byte)3 : s.Status;
        }

        return tasks;
    }

    public async Task<List<StudyTask>> GetAllTasks(CancellationToken cancellationToken = default)
    {
        return await DecorateAsync(db.Tasks.AsNoTracking().OrderBy(x => x.Deadline), null, cancellationToken);
    }

    public async Task<List<StudyTask>> GetTasksByUserIdAsync(int UserId, CancellationToken cancellationToken = default)
    {
        return await DecorateAsync(db.Tasks.AsNoTracking().Where(x => x.UserId == UserId).OrderBy(x => x.Deadline), UserId, cancellationToken);
    }

    public async Task<StudyTask?> GetByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return (await DecorateAsync(db.Tasks.AsNoTracking().Where(x => x.TaskId == taskId), null, cancellationToken)).FirstOrDefault();
    }

    public async Task<List<StudyTask>> GetVisibleTasksForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await DecorateAsync(db.Tasks.AsNoTracking().Where(x => x.UserId == userId || x.CreatedBy == userId || db.ClassMembers.Any(m => m.ClassId == x.ClassId && m.UserId == userId)).OrderBy(x => x.Deadline), userId, cancellationToken);
    }

    public async Task<List<StudyTask>> GetTasksByClassIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        return await DecorateAsync(db.Tasks.AsNoTracking().Where(x => x.ClassId == classId).OrderBy(x => x.Deadline), null, cancellationToken);
    }

    public async Task UpdateAsync(StudyTask task, CancellationToken cancellationToken = default)
    {
        Input.Task(task);
        var t = await db.Tasks.SingleAsync(x => x.TaskId == task.TaskId, cancellationToken);
        t.Titel = task.Titel;
        t.Subject = task.Subject;
        t.Description = task.Description;
        t.Deadline = task.Deadline;
        t.Priority = task.Priority;
        t.ClassId = task.ClassId;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HinzufugenTask(StudyTask task, CancellationToken cancellationToken = default)
    {
        Input.Task(task);
        var t = new StudyTask
        {
            UserId = task.UserId,
            CreatedBy = task.CreatedBy,
            ClassId = task.ClassId,
            Titel = task.Titel,
            Subject = task.Subject,
            Description = task.Description,
            Deadline = task.Deadline,
            Priority = task.Priority,
            Status = task.Status,
            IsRecurring = task.IsRecurring
        };
        db.Tasks.Add(t);
        await db.SaveChangesAsync(cancellationToken);
        task.TaskId = t.TaskId;
    }

    public async Task StatusAendernAsync(int taskId, byte neuerStatus, CancellationToken cancellationToken = default)
    {
        if (neuerStatus > 2)
            throw new ArgumentException("Invalid status.");
        await db.Tasks.Where(x => x.TaskId == taskId).ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, neuerStatus), cancellationToken);
    }

    public async Task LoeschenAsync(int taskId, CancellationToken cancellationToken = default)
    {
        await db.Tasks.Where(x => x.TaskId == taskId).ExecuteDeleteAsync(cancellationToken);
    }
}
