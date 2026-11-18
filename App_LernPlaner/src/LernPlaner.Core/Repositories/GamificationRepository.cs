using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core.Repositories;
public sealed class GamificationRepository(LernPlanerDbContext db)
{
    public async Task<List<Badge>> GetAllBadgesAsync(CancellationToken cancellationToken = default)
    {
        return await db.Badges.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<UserBadge>> GetUserBadgesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.UserBadges.AsNoTracking().Include(x => x.Badge).Where(x => x.UserId == userId).OrderByDescending(x => x.DateEarned).ToListAsync(cancellationToken);
    }

    public async Task<bool> HasBadgeAsync(int userId, int badgeId, CancellationToken cancellationToken = default)
    {
        return await db.UserBadges.AnyAsync(x => x.UserId == userId && x.BadgeId == badgeId, cancellationToken);
    }

    public async Task<int?> GetBadgeIdByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await db.Badges.Where(x => x.Name == name).Select(x => (int? )x.BadgeId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AwardBadgeAsync(int userId, int badgeId, CancellationToken cancellationToken = default)
    {
        db.UserBadges.Add(new UserBadge { UserId = userId, BadgeId = badgeId, DateEarned = DateTime.Now });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<User>> GetLeaderboardAsync(int classId, CancellationToken cancellationToken = default)
    {
        return await db.Users.AsNoTracking().Where(x => db.ClassMembers.Any(m => m.ClassId == classId && m.UserId == x.UserId && m.Role == 2)).OrderByDescending(x => x.XPPoints).ToListAsync(cancellationToken);
    }
}
