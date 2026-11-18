using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.RateLimiting;
using App_LernPlaner.Models;
using LernPlaner.Api;
using LernPlaner.Core;
using LernPlaner.Core.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(k =>
{
    k.Limits.MaxRequestBodySize = 128 * 1024;
    k.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddScoped(_ => DatabaseSettings.Create(builder.Configuration.GetConnectionString("LernPlaner") ?? builder.Configuration["LERNPLANER_DB"]));
builder.Services.AddAuthentication("Session").AddScheme<AuthenticationSchemeOptions, SessionAuthentication>("Session", _ =>
{
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.AddPolicy("login", ctx => RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx => RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var app = builder.Build();
// Schema changes are explicit operator commands, never performed by client startup.
if (args.Contains("--migrate") || args.Contains("--bootstrap-admin"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<LernPlanerDbContext>();
    if (args.Contains("--migrate"))
        await db.Database.MigrateAsync();
    if (args.Contains("--bootstrap-admin"))
    {
        if (await db.Users.AnyAsync(x => x.Role == Roles.Admin))
            throw new InvalidOperationException("An administrator already exists.");
        var username = builder.Configuration["LERNPLANER_ADMIN_USER"] ?? throw new InvalidOperationException("Set LERNPLANER_ADMIN_USER.");
        var password = builder.Configuration["LERNPLANER_ADMIN_PASSWORD"] ?? throw new InvalidOperationException("Set LERNPLANER_ADMIN_PASSWORD.");
        var email = builder.Configuration["LERNPLANER_ADMIN_EMAIL"] ?? throw new InvalidOperationException("Set LERNPLANER_ADMIN_EMAIL.");
        Input.Password(password);
        await new UserRepository(db).InsertAsync(username, BCrypt.Net.BCrypt.HashPassword(password), email, Roles.Admin);
    }

    return;
}

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.CacheControl = "no-store";
    if (!ctx.Request.IsHttps)
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsJsonAsync(new { error = "HTTPS is required." });
        return;
    }

    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ctx.RequestAborted);
    timeout.CancelAfter(TimeSpan.FromSeconds(25));
    ctx.RequestAborted = timeout.Token;
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        if (ctx.Response.HasStarted)
            throw;
        ctx.Response.StatusCode = ex switch
        {
            ForbiddenException => 403,
            ArgumentException or JsonException or BadHttpRequestException => 400,
            DbUpdateException => 409,
            OperationCanceledException => 504,
            SqlException or InvalidOperationException => 503,
            _ => 500
        };
        app.Logger.LogWarning("Request {TraceId} failed ({ExceptionType}).", ctx.TraceIdentifier, ex.GetType().Name);
        await ctx.Response.WriteAsJsonAsync(new { error = "The request could not be completed.", traceId = ctx.TraceIdentifier }, CancellationToken.None);
    }
});
if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "running" }));
app.MapPost("/auth/login", async (LoginRequest request, LernPlanerDbContext db, CancellationToken ct) =>
{
    Input.Text(request.Username, 100);
    Input.Text(request.Password, 72);
    var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.UserName == request.Username.Trim(), ct);
    // A fixed dummy hash avoids skipping the expensive password verification for unknown accounts.
    var hash = user?.PasswordHash ?? "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";
    bool valid = false;
    try
    {
        valid = BCrypt.Net.BCrypt.Verify(request.Password, hash);
    }
    catch (BCrypt.Net.SaltParseException)
    {
    }

    if (user == null || !valid || !user.IsActive)
        return Results.Unauthorized();
    await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
    await db.ApiSessions.Where(x => x.ExpiresAt < DateTime.UtcNow).ExecuteDeleteAsync(ct);
    await new RewardService(db).DailyAsync(user.UserId, ct);
    var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    var expires = DateTime.UtcNow.AddHours(1);
    db.ApiSessions.Add(new ApiSession { TokenHash = SessionAuthentication.Digest(token), UserId = user.UserId, ExpiresAt = expires, CredentialStamp = SessionAuthentication.Digest(user.PasswordHash) });
    await db.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);
    user = await db.Users.AsNoTracking().SingleAsync(x => x.UserId == user.UserId, ct);
    return Results.Ok(new LoginResponse(token, expires, user));
}).RequireRateLimiting("login");
app.MapPost("/auth/logout", async (HttpContext ctx, LernPlanerDbContext db, CancellationToken ct) =>
{
    var hash = SessionAuthentication.Digest(ctx.Request.Headers.Authorization.ToString()[7..]);
    await db.ApiSessions.Where(x => x.TokenHash == hash).ExecuteDeleteAsync(ct);
    return Results.Ok(true);
}).RequireAuthorization();
app.MapPost("/auth/password", async (PasswordRequest request, HttpContext ctx, LernPlanerDbContext db, CancellationToken ct) =>
{
    var actor = (User)ctx.Items["actor"]!;
    if (request.UserId != actor.UserId)
        throw new ForbiddenException();
    Input.Password(request.NewPassword);
    Input.Text(request.CurrentPassword, 72);
    if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, actor.PasswordHash))
        return Results.BadRequest();
    await using var tx = await db.Database.BeginTransactionAsync(ct);
    await new UserRepository(db).UpdatePasswordAsync(actor.UserId, BCrypt.Net.BCrypt.HashPassword(request.NewPassword), ct);
    await db.ApiSessions.Where(x => x.UserId == actor.UserId).ExecuteDeleteAsync(ct);
    await tx.CommitAsync(ct);
    return Results.Ok(true);
}).RequireAuthorization();
app.MapPost("/auth/register", async (RegisterRequest request, HttpContext ctx, LernPlanerDbContext db, CancellationToken ct) =>
{
    if (!((User)ctx.Items["actor"]!).IsAdmin)
        throw new ForbiddenException();
    if (request.Role != Roles.Lehrer && request.Role != Roles.Schueler)
        throw new ArgumentException("Invalid role.");
    Input.Password(request.Password);
    var repo = new UserRepository(db);
    var id = await repo.InsertAsync(request.Username, BCrypt.Net.BCrypt.HashPassword(request.Password), request.Email, request.Role, ct);
    return Results.Ok(await repo.GetByIdAsync(id, ct));
}).RequireAuthorization();
app.MapPost("/classes/join", async (JoinRequest request, HttpContext ctx, LernPlanerDbContext db, CancellationToken ct) =>
{
    var actor = (User)ctx.Items["actor"]!;
    if (request.UserId != actor.UserId || !actor.IsStudent)
        throw new ForbiddenException();
    Input.Text(request.JoinCode, 32);
    var c = await db.Classes.SingleOrDefaultAsync(x => x.JoinCode == request.JoinCode.Trim(), ct) ?? throw new ArgumentException("Invalid join code.");
    await new ClassRepository(db).AddMemberAsync(c.ClassId, actor.UserId, 2, ct);
    return Results.Ok(true);
}).RequireAuthorization();
app.MapPost("/classes/statistics", async ([Microsoft.AspNetCore.Mvc.FromBody] int classId, HttpContext ctx, LernPlanerDbContext db, CancellationToken ct) =>
{
    var actor = (User)ctx.Items["actor"]!;
    if (!actor.IsAdmin && !await db.ClassMembers.AnyAsync(x => x.ClassId == classId && x.UserId == actor.UserId, ct))
        throw new ForbiddenException();
    var c = await db.Classes.AsNoTracking().SingleAsync(x => x.ClassId == classId, ct);
    int students = await db.ClassMembers.CountAsync(x => x.ClassId == classId && x.Role == 2, ct);
    int tasks = await db.Tasks.CountAsync(x => x.ClassId == classId, ct);
    int submissions = await db.Submissions.CountAsync(s => db.Tasks.Any(t => t.TaskId == s.TaskId && t.ClassId == classId), ct);
    return Results.Ok(new { ClassName = c.Name, StudentCount = students, TaskCount = tasks, SubmissionCount = submissions, SubmissionRate = tasks == 0 ? 0 : Math.Round(100.0 * submissions / (tasks * Math.Max(students, 1)), 1) });
}).RequireAuthorization();
app.MapPost("/operations/{operation}", async (string operation, JsonElement[] body, HttpContext ctx, LernPlanerDbContext db, CancellationToken ct) =>
{
    var a = new Arguments(operation, body);
    var actor = (User)ctx.Items["actor"]!;
    await new OperationPolicy(db, actor).DemandAsync(operation, a, ct);
    object? result;
    if (operation == "Class.GetAllAsync" && !actor.IsAdmin)
        result = await new ClassRepository(db).GetForUserAsync(actor.UserId, ct);
    else if (operation == "Task.GetAllTasks" && !actor.IsAdmin)
        result = await new TaskRepository(db).GetVisibleTasksForUserAsync(actor.UserId, ct);
    else if (operation == "Submission.GetForUserAsync" && !actor.IsAdmin && a.Get<int>("userId") != actor.UserId)
        result = await db.Submissions.AsNoTracking().Where(s => s.UserId == a.Get<int>("userId") && db.Tasks.Any(t => t.TaskId == s.TaskId && (t.CreatedBy == actor.UserId || db.Classes.Any(c => c.ClassId == t.ClassId && c.TeacherId == actor.UserId)))).ToListAsync(ct);
    else
        result = await ExecuteWithEffects(operation, a, db, ct);
    // Never return password hashes. Other users' private account fields are also omitted.
    if (result is User u)
        result = DirectoryUser(u, actor);
    if (result is List<User> users)
        result = users.Select(u => DirectoryUser(u, actor)).ToList();
    return Results.Json(result);
}).RequireAuthorization();
await app.RunAsync();
static User DirectoryUser(User u, User actor) => actor.IsAdmin || u.UserId == actor.UserId ? u : new User
{
    UserId = u.UserId,
    UserName = u.UserName,
    FirstName = u.FirstName,
    LastName = u.LastName,
    Role = u.Role,
    XPPoints = u.XPPoints,
    ProfilePicture = u.ProfilePicture,
    IsActive = u.IsActive
};
static async Task<object?> ExecuteWithEffects(string op, Arguments a, LernPlanerDbContext db, CancellationToken ct)
{
    var reward = new RewardService(db);
    if (op == "Submission.SaveProgressAsync" || op == "Submission.GradeAsync" || op == "Study.LogSessionAsync" || op == "Task.HinzufugenTask")
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        Submission? before = null;
        if (op == "Submission.SaveProgressAsync")
        {
            var s = a.Get<Submission>("submission");
            before = await db.Submissions.AsNoTracking().SingleOrDefaultAsync(x => x.TaskId == s.TaskId && x.UserId == s.UserId, ct);
            if (before?.Status >= 2 && s.Status < 2)
                throw new ArgumentException("Completed submissions cannot be reopened.");
        }

        if (op == "Submission.GradeAsync")
            before = await db.Submissions.AsNoTracking().SingleAsync(x => x.SubmissionId == a.Get<int>("submissionId"), ct);
        var result = await OperationDispatcher.ExecuteAsync(op, a, db, ct);
        if (op == "Submission.SaveProgressAsync")
        {
            var s = a.Get<Submission>("submission");
            if (s.Status == 2 && (before == null || before.Status < 2))
            {
                await reward.SubmissionAsync(s.UserId, s.TaskId, ct);
                await reward.AddAsync(s.UserId, s.LearningMinutes / 60 * 15, ct);
            }
        }

        if (op == "Submission.GradeAsync" && !before!.IsGraded)
            await reward.GradeAsync(before, a.Get<string>("grade"), ct);
        if (op == "Study.LogSessionAsync")
            await reward.StudyAsync(a.Get<int>("userId"), a.Get<int>("durationMin"), ct);
        if (op == "Task.HinzufugenTask")
        {
            var t = a.Get<StudyTask>("task");
            if (t.ClassId.HasValue)
            {
                var ids = await db.ClassMembers.Where(x => x.ClassId == t.ClassId && x.Role == 2).Select(x => x.UserId).ToListAsync(ct);
                foreach (var id in ids)
                    await reward.NotifyAsync(id, $"Neue Aufgabe: {t.Titel}", ct);
            }
        }

        await tx.CommitAsync(ct);
        return result;
    }

    return await OperationDispatcher.ExecuteAsync(op, a, db, ct);
}

public partial class Program;
