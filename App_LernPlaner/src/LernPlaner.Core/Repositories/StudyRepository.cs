using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core.Repositories;
public sealed class StudyRepository(LernPlanerDbContext db)
{
    public async Task LogSessionAsync(int userId, int taskId, int durationMin, string notes, CancellationToken cancellationToken = default)
    {
        if (durationMin < 1 || durationMin > 1440)
            throw new ArgumentException("Duration must be 1-1440 minutes.");
        Input.Text(notes, 10000, true);
        db.StudySessions.Add(new StudySession { UserId = userId, TaskId = taskId == 0 ? null : taskId, DurationMin = durationMin, Notes = notes, StartTime = DateTime.Now });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<StudySession>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.StudySessions.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.StartTime).ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalMinutesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.StudySessions.Where(x => x.UserId == userId).SumAsync(x => (int? )x.DurationMin, cancellationToken) ?? 0;
    }
}
