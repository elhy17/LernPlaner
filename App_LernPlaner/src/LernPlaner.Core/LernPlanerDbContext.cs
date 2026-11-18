using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.SqlClient;

namespace LernPlaner.Core;
public sealed class ApiSession
{
    public string TokenHash { get; set; } = "";
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string CredentialStamp { get; set; } = "";
}

public sealed class LernPlanerDbContext(DbContextOptions<LernPlanerDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<StudyTask> Tasks => Set<StudyTask>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassMember> ClassMembers => Set<ClassMember>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();
    public DbSet<ApiSession> ApiSessions => Set<ApiSession>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        var u = m.Entity<User>();
        u.ToTable("Users");
        u.HasKey(x => x.UserId);
        u.Property(x => x.UserName).HasColumnName("Username").HasMaxLength(100).UseCollation("Latin1_General_100_CI_AS_SC");
        u.Property(x => x.Email).HasMaxLength(254).UseCollation("Latin1_General_100_CI_AS_SC");
        u.Property(x => x.PasswordHash).HasMaxLength(100);
        u.Property(x => x.Role).HasMaxLength(20);
        u.Property(x => x.ProfilePicture).HasColumnName("ProfilePicturePath").HasMaxLength(1024);
        u.Property(x => x.FirstName).HasMaxLength(100);
        u.Property(x => x.LastName).HasMaxLength(100);
        u.Property(x => x.ClassName).HasMaxLength(200);
        u.HasIndex(x => x.UserName).IsUnique();
        u.HasIndex(x => x.Email).IsUnique();
        var c = m.Entity<Class>();
        c.ToTable("Classes");
        c.HasKey(x => x.ClassId);
        c.Property(x => x.Name).HasMaxLength(200);
        c.Property(x => x.JoinCode).HasMaxLength(32).UseCollation("Latin1_General_100_CI_AS_SC");
        c.HasIndex(x => x.JoinCode).IsUnique();
        c.HasOne<User>().WithMany().HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.NoAction);
        var cm = m.Entity<ClassMember>();
        cm.ToTable("ClassMembers", t => t.HasCheckConstraint("CK_ClassMember_Role", "[Role] IN (1,2)"));
        cm.HasKey(x => new { x.ClassId, x.UserId });
        cm.HasOne<Class>().WithMany(x => x.Members).HasForeignKey(x => x.ClassId).OnDelete(DeleteBehavior.Cascade);
        cm.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        var t = m.Entity<StudyTask>();
        t.ToTable("Tasks", t =>
        {
            t.HasCheckConstraint("CK_Task_Priority", "[Priority] BETWEEN 1 AND 3");
            t.HasCheckConstraint("CK_Task_Status", "[Status] BETWEEN 0 AND 2");
        });
        t.HasKey(x => x.TaskId);
        t.Property(x => x.Titel).HasColumnName("Title").HasMaxLength(300);
        t.Property(x => x.Subject).HasMaxLength(150);
        t.Property(x => x.Description).HasMaxLength(10000);
        t.Ignore(x => x.ButtonText);
        t.Ignore(x => x.CreatedByUsername);
        t.HasOne<Class>().WithMany(x => x.Tasks).HasForeignKey(x => x.ClassId).OnDelete(DeleteBehavior.NoAction);
        t.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        t.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.NoAction);
        t.HasIndex(x => new { x.ClassId, x.Deadline });
        var sub = m.Entity<Submission>();
        sub.ToTable("Submissions", t =>
        {
            t.HasCheckConstraint("CK_Submission_Status", "[Status] BETWEEN 0 AND 3");
            t.HasCheckConstraint("CK_Submission_Minutes", "[LearningMinutes] BETWEEN 0 AND 1440");
        });
        sub.HasKey(x => x.SubmissionId);
        sub.HasIndex(x => new { x.TaskId, x.UserId }).IsUnique();
        sub.Property(x => x.Grade).HasMaxLength(10);
        sub.Property(x => x.Feedback).HasMaxLength(10000);
        sub.Property(x => x.Notizen).HasMaxLength(10000);
        sub.Property(x => x.Abgabe).HasMaxLength(10000);
        sub.Property(x => x.FilePath).HasMaxLength(1024);
        sub.HasOne<StudyTask>().WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        sub.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        var st = m.Entity<StudySession>();
        st.ToTable("StudySessions", t => t.HasCheckConstraint("CK_Study_Minutes", "[DurationMin] BETWEEN 1 AND 1440"));
        st.HasKey(x => x.SessionId);
        st.Property(x => x.Notes).HasMaxLength(10000);
        st.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        st.HasOne<StudyTask>().WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.SetNull);
        var n = m.Entity<Notification>();
        n.ToTable("Notifications");
        n.HasKey(x => x.NotifId);
        n.Property(x => x.Message).HasMaxLength(2000);
        n.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        n.HasIndex(x => new { x.UserId, x.IsRead });
        var msg = m.Entity<Message>();
        msg.ToTable("Messages");
        msg.HasKey(x => x.MessageId);
        msg.Ignore(x => x.SenderName);
        msg.Property(x => x.Body).HasMaxLength(10000);
        msg.HasOne<User>().WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.NoAction);
        msg.HasOne<User>().WithMany().HasForeignKey(x => x.ReceiverId).OnDelete(DeleteBehavior.NoAction);
        msg.HasIndex(x => new { x.ReceiverId, x.SentAt });
        var b = m.Entity<Badge>();
        b.ToTable("Badges");
        b.HasKey(x => x.BadgeId);
        b.Property(x => x.Name).HasMaxLength(100);
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.IconPath).HasMaxLength(1024);
        b.HasData(new Badge { BadgeId = 1, Name = "Erste Abgabe", Description = "Erste Abgabe", IconPath = "", XPReward = 10 }, new Badge { BadgeId = 2, Name = "Fleißiger Schüler", Description = "5 pünktliche Abgaben", IconPath = "", XPReward = 25 }, new Badge { BadgeId = 3, Name = "Streber", Description = "Note 1", IconPath = "", XPReward = 25 }, new Badge { BadgeId = 4, Name = "Lernmarathon", Description = "600 Lernminuten", IconPath = "", XPReward = 50 });
        var ub = m.Entity<UserBadge>();
        ub.ToTable("UserBadge");
        ub.HasKey(x => new { x.UserId, x.BadgeId });
        ub.Property(x => x.DateEarned).HasColumnName("EarnedAt");
        ub.HasOne(x => x.Badge).WithMany().HasForeignKey(x => x.BadgeId);
        ub.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
        var s = m.Entity<SubTask>();
        s.ToTable("SubTasks");
        s.HasKey(x => x.SubTaskId);
        s.Property(x => x.Title).HasMaxLength(300);
        s.HasOne<StudyTask>().WithMany(x => x.SubTasks).HasForeignKey(x => x.TaskId);
        var tag = m.Entity<Tag>();
        tag.ToTable("Tags");
        tag.HasKey(x => x.TagId);
        tag.Property(x => x.Name).HasMaxLength(100);
        tag.Property(x => x.color).HasMaxLength(20);
        t.HasMany(x => x.Tags).WithMany().UsingEntity<TaskTag>(r => r.HasOne<Tag>().WithMany().HasForeignKey(x => x.TagId), l => l.HasOne<StudyTask>().WithMany().HasForeignKey(x => x.TaskId), j =>
        {
            j.ToTable("TaskTags");
            j.HasKey(x => new { x.TaskId, x.TagId });
        });
        var a = m.Entity<ApiSession>();
        a.HasKey(x => x.TokenHash);
        a.Property(x => x.TokenHash).HasMaxLength(64);
        a.Property(x => x.CredentialStamp).HasMaxLength(64);
        a.HasIndex(x => x.ExpiresAt);
        a.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
        // Consistent Unicode/case-insensitive account and join-code matching on SQL Server.
        m.UseCollation("Latin1_General_100_CI_AS_SC");
    }
}

public static class DatabaseSettings
{
    public static LernPlanerDbContext Create(string? connection = null)
    {
        connection ??= Environment.GetEnvironmentVariable("LERNPLANER_DB");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Set LERNPLANER_DB (SQL Server). Remote clients only need LERNPLANER_API_URL.");
        var cs = new SqlConnectionStringBuilder(connection);
        if (!cs.IntegratedSecurity && string.IsNullOrWhiteSpace(cs.Password))
            throw new InvalidOperationException("SQL authentication requires a secret password.");
        if (!cs.Encrypt || cs.TrustServerCertificate)
        {
            if (!cs.DataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("SQL Server requires Encrypt=True;TrustServerCertificate=False outside LocalDB.");
        }

        return new(new DbContextOptionsBuilder<LernPlanerDbContext>().UseSqlServer(cs.ConnectionString, o => o.CommandTimeout(20)).Options);
    }
}

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<LernPlanerDbContext>
{
    public LernPlanerDbContext CreateDbContext(string[] args) => DatabaseSettings.Create();
}
