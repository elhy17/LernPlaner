using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core;
/// <summary>Called within the server transaction, never from client-supplied reward amounts.</summary>
public sealed class RewardService(LernPlanerDbContext db)
{
    public async Task AddAsync(int userId, int xp, CancellationToken ct)
    {
        if (xp > 0)
            await db.Users.Where(x => x.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(x => x.XPPoints, x => x.XPPoints + xp), ct);
    }

    public async Task NotifyAsync(int userId, string message, CancellationToken ct)
    {
        db.Notifications.Add(new Notification { UserId = userId, Message = message, TriggerAt = DateTime.Now });
        await db.SaveChangesAsync(ct);
    }

    public async Task BadgeAsync(int userId, string name, CancellationToken ct)
    {
        var b = await db.Badges.SingleAsync(x => x.Name == name, ct);
        if (await db.UserBadges.AnyAsync(x => x.UserId == userId && x.BadgeId == b.BadgeId, ct))
            return;
        db.UserBadges.Add(new UserBadge { UserId = userId, BadgeId = b.BadgeId, DateEarned = DateTime.Now });
        await db.SaveChangesAsync(ct);
        await AddAsync(userId, b.XPReward, ct);
        await NotifyAsync(userId, $"Neues Badge erhalten: {b.Name} ({b.IconPath})", ct);
    }

    public async Task DailyAsync(int userId, CancellationToken ct)
    {
        var today = DateTime.Today;
        if (await db.Notifications.AnyAsync(x => x.UserId == userId && x.Message == "__daily_login__" && x.TriggerAt >= today, ct))
            return;
        db.Notifications.Add(new Notification { UserId = userId, Message = "__daily_login__", TriggerAt = DateTime.Now, IsRead = true });
        await db.SaveChangesAsync(ct);
        await AddAsync(userId, 10, ct);
    }

    public async Task SubmissionAsync(int userId, int taskId, CancellationToken ct)
    {
        var t = await db.Tasks.AsNoTracking().SingleAsync(x => x.TaskId == taskId, ct);
        await AddAsync(userId, DateTime.Now > t.Deadline ? 5 : 20, ct);
        var count = await db.Submissions.CountAsync(x => x.UserId == userId && x.Status >= 2, ct);
        if (count == 1)
            await BadgeAsync(userId, "Erste Abgabe", ct);
        var ontime = await db.Submissions.CountAsync(x => x.UserId == userId && x.Status >= 2 && db.Tasks.Any(t => t.TaskId == x.TaskId && x.SubmittedAt <= t.Deadline), ct);
        if (ontime >= 5)
            await BadgeAsync(userId, "Fleißiger Schüler", ct);
        if (t.CreatedBy is int creator && creator != userId)
            await NotifyAsync(creator, $"Neue Abgabe für: {t.Titel}", ct);
    }

    public async Task StudyAsync(int userId, int minutes, CancellationToken ct)
    {
        await AddAsync(userId, minutes / 60 * 15, ct);
        if (await db.StudySessions.Where(x => x.UserId == userId).SumAsync(x => (int? )x.DurationMin, ct) >= 600)
            await BadgeAsync(userId, "Lernmarathon", ct);
    }

    public async Task GradeAsync(Submission s, string grade, CancellationToken ct)
    {
        if (grade == "1")
        {
            await AddAsync(s.UserId, 50, ct);
            await BadgeAsync(s.UserId, "Streber", ct);
        }
        else if (grade == "2")
            await AddAsync(s.UserId, 30, ct);
        var t = await db.Tasks.AsNoTracking().SingleAsync(x => x.TaskId == s.TaskId, ct);
        await NotifyAsync(s.UserId, $"Du hast eine Bewertung erhalten: {t.Titel} (Note {grade})", ct);
    }
}
