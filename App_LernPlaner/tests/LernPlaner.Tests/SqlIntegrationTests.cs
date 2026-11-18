using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using App_LernPlaner.Models;
using App_LernPlaner.Remote;
using LernPlaner.Core;
using LernPlaner.Core.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace LernPlaner.Tests;
public sealed class SqlFactAttribute : FactAttribute
{
    public SqlFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("LERNPLANER_SKIP_SQL_TESTS") == "1")
            Skip = "SQL tests explicitly disabled. Run again against SQL Server before deployment.";
    }
}

public sealed class ApiFactory(string connection) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(s =>
        {
            s.RemoveAll<LernPlanerDbContext>();
            s.AddScoped(_ => DatabaseSettings.Create(connection));
        });
    }

    public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
}

public partial class SqlIntegrationTests
{
    private static readonly string Hash = BCrypt.Net.BCrypt.HashPassword("Test-password-123");
    private static string Connection(string name)
    {
        var template = Environment.GetEnvironmentVariable("LERNPLANER_TEST_SQL") ?? "Server=(localdb)\\MSSQLLocalDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        var b = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(template)
        {
            InitialCatalog = name
        };
        return b.ConnectionString;
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        public string Name { get; } = "LernPlaner_Test_" + Guid.NewGuid().ToString("N");
        public string ConnectionString => Connection(Name);

        public LernPlanerDbContext Open() => DatabaseSettings.Create(ConnectionString);
        public static async Task<TestDatabase> Create()
        {
            var t = new TestDatabase();
            await using var db = t.Open();
            await db.Database.MigrateAsync();
            Assert.True(await db.Database.CanConnectAsync());
            return t;
        }

        public async ValueTask DisposeAsync()
        {
            Assert.StartsWith("LernPlaner_Test_", Name);
            await using var db = Open();
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static async Task<User> User(LernPlanerDbContext db, string name, string role = Roles.Schueler)
    {
        var id = await new UserRepository(db).InsertAsync(name, Hash, name + "@example.test", role);
        return (await new UserRepository(db).GetByIdAsync(id))!;
    }

    private static async Task<LoginResponse> Login(HttpClient c, string name)
    {
        using var r = await c.PostAsJsonAsync("/auth/login", new LoginRequest(name, "Test-password-123"));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await r.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        var login = (await r.Content.ReadFromJsonAsync<LoginResponse>())!;
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        return login;
    }

    private static Task<HttpResponseMessage> Op(HttpClient c, string op, params object? [] args) => c.PostAsJsonAsync("/operations/" + op, args);
    [SqlFact]
    public async Task SqlServerMigrationsAndRepositoriesPerformRealCrud()
    {
        await using var test = await TestDatabase.Create();
        await using var db = test.Open();
        Assert.Equal(db.Database.GetMigrations().Count(), (await db.Database.GetAppliedMigrationsAsync()).Count());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        var teacher = await User(db, "teacher", Roles.Lehrer);
        var student = await User(db, "student");
        var classes = new ClassRepository(db);
        var cls = await classes.CreateAsync("Math", teacher.UserId, "JOIN123");
        await classes.AddMemberAsync(cls.ClassId, student.UserId, 2);
        Assert.Equal(1, await classes.CountStudentsAsync(cls.ClassId));
        Assert.True(await classes.IsMemberAsync(cls.ClassId, teacher.UserId));
        await Assert.ThrowsAsync<DbUpdateException>(() => classes.CreateAsync("Duplicate", teacher.UserId, "join123"));
        db.ChangeTracker.Clear();
        Assert.Equal(1, await db.Classes.CountAsync());
        Assert.Equal(2, await db.ClassMembers.CountAsync());
        var tasks = new TaskRepository(db);
        var task = new StudyTask
        {
            Titel = "Algebra",
            Subject = "Math",
            UserId = teacher.UserId,
            CreatedBy = teacher.UserId,
            ClassId = cls.ClassId,
            Priority = 2,
            Deadline = DateTime.Now.AddDays(2)
        };
        await tasks.HinzufugenTask(task);
        Assert.True(task.TaskId > 0);
        task.Titel = "Updated";
        await tasks.UpdateAsync(task);
        Assert.Equal("Updated", (await tasks.GetByIdAsync(task.TaskId))!.Titel);
        Assert.Single(await tasks.GetVisibleTasksForUserAsync(student.UserId));
        var submissions = new SubmissionRepository(db);
        await submissions.SaveProgressAsync(new Submission { TaskId = task.TaskId, UserId = student.UserId, Status = 2, Abgabe = "Answer" });
        await submissions.GradeAsync((await submissions.GetByTaskAndUserAsync(task.TaskId, student.UserId))!.SubmissionId, "1", "Good");
        Assert.Equal("1", (await submissions.GetForUserAsync(student.UserId)).Single().Grade);
        var study = new StudyRepository(db);
        await study.LogSessionAsync(student.UserId, task.TaskId, 60, "Revision");
        Assert.Equal(60, await study.GetTotalMinutesAsync(student.UserId));
        var messages = new MessageRepository(db);
        var messageId = await messages.SendAsync(student.UserId, teacher.UserId, "Hi");
        Assert.Equal(1, await messages.CountUnreadTotalAsync(teacher.UserId));
        await messages.EditAsync(messageId, student.UserId, "Edited");
        await messages.MarkConversationReadAsync(teacher.UserId, student.UserId);
        Assert.Equal(0, await messages.CountUnreadTotalAsync(teacher.UserId));
        await messages.SoftDeleteAsync(messageId, student.UserId);
        Assert.True((await messages.GetConversationAsync(student.UserId, teacher.UserId)).Single().IsDeleted);
        var notifications = new NotificationRepository(db);
        await notifications.InsertAsync(student.UserId, "Test");
        await notifications.MarkAllAsReadAsync(student.UserId);
        Assert.Equal(0, await notifications.CountUnreadAsync(student.UserId));
        var badges = new GamificationRepository(db);
        await badges.AwardBadgeAsync(student.UserId, 1);
        Assert.True(await badges.HasBadgeAsync(student.UserId, 1));
        await classes.DeleteAsync(cls.ClassId);
        db.ChangeTracker.Clear();
        Assert.Null((await tasks.GetByIdAsync(task.TaskId))!.ClassId);
        Assert.Empty(await db.ClassMembers.ToListAsync());
        await tasks.LoeschenAsync(task.TaskId);
        Assert.Null(await tasks.GetByIdAsync(task.TaskId));
        Assert.Empty(await submissions.GetForUserAsync(student.UserId));
        Assert.Null((await study.GetForUserAsync(student.UserId)).Single().TaskId);
    }

    [SqlFact]
    public async Task ApiAndActualWpfHttpAdapterEnforcePermissionsAndPersistData()
    {
        await using var test = await TestDatabase.Create();
        int sid, tid, outsiderId, adminId;
        Class cls;
        StudyTask task;
        await using (var db = test.Open())
        {
            var teacher = await User(db, "teacher", Roles.Lehrer);
            var student = await User(db, "student");
            sid = student.UserId;
            tid = teacher.UserId;
            outsiderId = (await User(db, "outsider")).UserId;
            adminId = (await User(db, "admin", Roles.Admin)).UserId;
            cls = await new ClassRepository(db).CreateAsync("Math", tid, "JOIN123");
            await new ClassRepository(db).AddMemberAsync(cls.ClassId, sid, 2);
            task = new StudyTask
            {
                UserId = tid,
                CreatedBy = tid,
                ClassId = cls.ClassId,
                Titel = "Algebra",
                Subject = "Math",
                Priority = 2,
                Deadline = DateTime.Now.AddDays(1)
            };
            await new TaskRepository(db).HinzufugenTask(task);
        }

        await using var api = new ApiFactory(test.ConnectionString);
        using var anonymous = api.Client();
        Assert.Equal(HttpStatusCode.Unauthorized, (await Op(anonymous, "Task.GetAllTasks")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/auth/login", new LoginRequest("student", "wrong"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/auth/login", new LoginRequest("unknown", "wrong"))).StatusCode);
        using var studentClient = api.Client();
        await Login(studentClient, "student");
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(studentClient, "User.UpdateRoleAsync", sid, Roles.Admin)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(studentClient, "User.UpdatePasswordAsync", sid, Hash)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(studentClient, "User.AddXpAsync", sid, 999)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(studentClient, "Class.AddMemberAsync", cls.ClassId, outsiderId, (byte)2)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(studentClient, "Task.LoeschenAsync", task.TaskId)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(studentClient, "Message.SendAsync", tid, sid, "spoof")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(studentClient, "Submission.GetForTaskAsync", task.TaskId)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Op(studentClient, "Unknown.Sql", "DROP TABLE Users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(studentClient, "User.UpdateUserProfileAsync", new UserProfile { UserId = tid, Email = "spoof@example.test" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Op(studentClient, "Study.GetForUserAsync", (object? )null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Op(studentClient, "Study.GetForUserAsync", "not-an-integer")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Op(studentClient, "Study.GetForUserAsync")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Op(studentClient, "Study.LogSessionAsync", sid, task.TaskId, -1, "")).StatusCode);
        var submit = new Submission
        {
            UserId = sid,
            TaskId = task.TaskId,
            Status = 2,
            Abgabe = "42",
            LearningMinutes = 60
        };
        Assert.Equal(HttpStatusCode.OK, (await Op(studentClient, "Submission.SaveProgressAsync", submit)).StatusCode);
        int xp;
        await using (var db = test.Open())
            xp = (await db.Users.AsNoTracking().SingleAsync(x => x.UserId == sid)).XPPoints;
        Assert.Equal(HttpStatusCode.OK, (await Op(studentClient, "Submission.SaveProgressAsync", submit)).StatusCode);
        await using (var db = test.Open())
            Assert.Equal(xp, (await db.Users.AsNoTracking().SingleAsync(x => x.UserId == sid)).XPPoints);
        using var outsider = api.Client();
        await Login(outsider, "outsider");
        Assert.Equal(HttpStatusCode.Forbidden, (await Op(outsider, "Task.GetByIdAsync", task.TaskId)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await outsider.PostAsJsonAsync("/classes/join", new JoinRequest("WRONG", outsiderId))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await outsider.PostAsJsonAsync("/classes/join", new JoinRequest("JOIN123", outsiderId))).StatusCode);
        using var teacherClient = api.Client();
        await Login(teacherClient, "teacher");
        var submissions = await (await Op(teacherClient, "Submission.GetForTaskAsync", task.TaskId)).Content.ReadFromJsonAsync<List<Submission>>();
        Assert.Single(submissions!);
        Assert.Equal(HttpStatusCode.OK, (await Op(teacherClient, "Submission.GradeAsync", submissions![0].SubmissionId, "1", "Good")).StatusCode);
        using var remote = new RemoteClient(new Uri("https://localhost"), api.Server.CreateHandler());
        Assert.NotNull(await remote.LoginAsync("student", "Test-password-123"));
        var visible = await remote.CallAsync<List<StudyTask>>("Task.GetVisibleTasksForUserAsync", [sid]);
        Assert.Single(visible);
        Assert.Equal((byte)3, visible[0].Status);
        await remote.CallAsync("Study.LogSessionAsync", [sid, task.TaskId, 30, "HTTP adapter"]);
        Assert.Equal(30, await remote.CallAsync<int>("Study.GetTotalMinutesAsync", [sid]));
        RemoteClient.Configure(remote);
        try
        {
            var auth = await new App_LernPlaner.Services.AuthService().TryLoginAsync("student", "Test-password-123");
            Assert.True(auth.Success);
            App_LernPlaner.Services.sessionManager.CurrentUser = auth.User;
            var wpfTasks = await new App_LernPlaner.Services.TaskService().GetVisibleTasksForUserAsync(auth.User!);
            Assert.Single(wpfTasks);
            var stats = await new App_LernPlaner.Services.StatisticsService().GetClassStatisticsAsync(cls);
            Assert.Equal(1, stats.TaskCount);
            await new App_LernPlaner.Services.StudyService().LogSessionAsync(sid, 0, 15, "WPF workflow");
            Assert.Equal(45, await new App_LernPlaner.Services.StudyService().GetTotalMinutesAsync(sid));
            Assert.Single(await new App_LernPlaner.Services.SubmissionService().GetSubmissionsForUserAsync(sid));
            await App_LernPlaner.Services.sessionManager.LogoutAsync();
            Assert.False(App_LernPlaner.Services.sessionManager.IsLoggedIn);
        }
        finally
        {
            RemoteClient.Configure(null);
        }

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => remote.CallAsync("Task.GetAllTasks", []));
        using var admin = api.Client();
        await Login(admin, "admin");
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsJsonAsync("/auth/register", new RegisterRequest("newuser", "Test-password-123", "new@example.test", Roles.Schueler))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsJsonAsync("/auth/register", new RegisterRequest("teacher2", "Test-password-123", "t2@example.test", Roles.Lehrer))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/auth/register", new RegisterRequest("bad", "tiny", "bad", Roles.Admin))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await studentClient.PostAsJsonAsync("/auth/password", new PasswordRequest(sid, "Test-password-123", "Changed-password-123"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Op(studentClient, "Task.GetAllTasks")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Op(admin, "User.UpdateActiveAsync", tid, false)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Op(teacherClient, "Task.GetAllTasks")).StatusCode);
        using var http = api.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost"), AllowAutoRedirect = false });
        Assert.Equal(HttpStatusCode.BadRequest, (await http.GetAsync("/health")).StatusCode);
    }

    [SqlFact]
    public async Task DatabaseConnectionFailureIsSafeThroughApi()
    {
        var connection = "Server=tcp:127.0.0.1,1;Database=Unavailable;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;Connect Timeout=1;ConnectRetryCount=0";
        await using var api = new ApiFactory(connection);
        using var client = api.Client();
        using var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest("user", "password"));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("127.0.0.1", json);
        Assert.DoesNotContain("SqlException", json);
    }
}
