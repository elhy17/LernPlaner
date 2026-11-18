using System.Net;
using System.Text;
using System.Text.Json;
using App_LernPlaner.Models;
using App_LernPlaner.Remote;
using LernPlaner.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LernPlaner.Tests;
public class UnitTests
{
    [Fact]
    public void ModelAndSqlServerScriptContainSchemaConstraints()
    {
        using var db = DatabaseSettings.Create("Server=(localdb)\\MSSQLLocalDB;Database=ModelOnly;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        var script = db.Database.GenerateCreateScript();
        Assert.Contains("CREATE TABLE [Tasks]", script);
        Assert.Contains("CK_Task_Priority", script);
        Assert.Contains("FOREIGN KEY", script);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Submissions_TaskId_UserId]", script);
        Assert.DoesNotContain("ButtonText", script);
        Assert.NotEmpty(db.Database.GetMigrations());
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public void PasswordHashCannotBeSerialized()
    {
        var json = JsonSerializer.Serialize(new User { PasswordHash = "secret" });
        Assert.DoesNotContain("secret", json);
        Assert.DoesNotContain("PasswordHash", json);
    }

    [Theory]
    [InlineData("http://localhost:7243")]
    [InlineData("https://user:secret@localhost")]
    public void ClientRequiresSecureUrl(string url) => Assert.Throws<ArgumentException>(() => new RemoteClient(new Uri(url)));
    [Fact]
    public void SqlSettingsRejectInsecureRemoteConnection() => Assert.Throws<InvalidOperationException>(() => DatabaseSettings.Create("Server=remote;Database=test;Integrated Security=True;Encrypt=False"));
    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void TaskPriorityIsValidated(byte priority) => Assert.Throws<ArgumentException>(() => Input.Task(new StudyTask { Titel = "x", Subject = "s", Deadline = DateTime.Today, Priority = priority }));
    [Fact]
    public async Task ClientReportsUnavailableServer()
    {
        using var c = new RemoteClient(new Uri("https://localhost"), new Handler((_, _) => throw new HttpRequestException("unavailable")));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => c.CallAsync("Task.GetAllTasks", []));
        Assert.Contains("nicht erreichbar", ex.Message);
    }

    [Fact]
    public async Task ClientReportsTimeout()
    {
        using var c = new RemoteClient(new Uri("https://localhost"), new Handler(async (_, ct) =>
        {
            await Task.Delay(1000, ct);
            return new(HttpStatusCode.OK);
        }), TimeSpan.FromMilliseconds(20));
        await Assert.ThrowsAsync<TimeoutException>(() => c.CallAsync("Task.GetAllTasks", []));
    }

    [Fact]
    public async Task CallerCancellationIsPreserved()
    {
        using var c = new RemoteClient(new Uri("https://localhost"), new Handler(async (_, ct) =>
        {
            await Task.Delay(1000, ct);
            return new(HttpStatusCode.OK);
        }));
        using var cancel = new CancellationTokenSource(20);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => c.CallAsync("Task.GetAllTasks", [], cancel.Token));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task ClientReportsAuthorizationFailure(HttpStatusCode status)
    {
        using var c = new RemoteClient(new Uri("https://localhost"), new Handler((_, _) => Task.FromResult(new HttpResponseMessage(status))));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => c.CallAsync("Task.GetAllTasks", []));
    }

    [Fact]
    public void WpfTaskCountersPreserveBehavior()
    {
        var service = new App_LernPlaner.Services.TaskService();
        var tasks = new List<StudyTask>
        {
            new()
            {
                Status = 0,
                Deadline = DateTime.Today
            },
            new()
            {
                Status = 1,
                Deadline = DateTime.Today.AddDays(1)
            },
            new()
            {
                Status = 2,
                Deadline = DateTime.Today
            }
        };
        Assert.Equal(1, service.CountOpenTasks(tasks));
        Assert.Equal(1, service.CountFinishedTasks(tasks));
        Assert.Equal(2, service.CountDueThisWeek(tasks));
    }

    internal sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => send(request, ct);
    }
}
