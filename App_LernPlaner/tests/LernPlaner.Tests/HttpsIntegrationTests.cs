using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using App_LernPlaner.Models;
using App_LernPlaner.Remote;
using LernPlaner.Core.Repositories;
using Xunit;

namespace LernPlaner.Tests;
public partial class SqlIntegrationTests
{
    [SqlFact]
    public async Task KestrelHttpsEndpointPerformsSqlServerOperationsOverRealTls()
    {
        await using var test = await TestDatabase.Create();
        int studentId;
        await using (var db = test.Open())
            studentId = (await User(db, "tlsstudent")).UserId;
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "LernPlaner.sln")))
            root = root.Parent;
        Assert.NotNull(root);
        string artifact = Path.Combine(root!.FullName, "artifacts", "tls-" + Guid.NewGuid().ToString("N") + ".pfx");
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName("localhost");
        request.CertificateExtensions.Add(names.Build());
        using var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));
        string password = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await File.WriteAllBytesAsync(artifact, cert.Export(X509ContentType.Pfx, password));
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var psi = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = root.FullName
        };
        var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
        psi.ArgumentList.Add(Path.Combine(root.FullName, "src", "LernPlaner.Api", "bin", configuration, "net8.0", "LernPlaner.Api.dll"));
        psi.ArgumentList.Add("--urls");
        psi.ArgumentList.Add($"https://localhost:{port}");
        psi.Environment["LERNPLANER_DB"] = test.ConnectionString;
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        psi.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = artifact;
        psi.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = password;
        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        try
        {
            // A test-only certificate pin. No system trust changes and no production validation bypass.
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, remote, _, _) => remote != null && remote.GetCertHashString(HashAlgorithmName.SHA256) == cert.GetCertHashString(HashAlgorithmName.SHA256)
            };
            using var health = new HttpClient(handler, disposeHandler: false)
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
            using var client = new RemoteClient(new Uri($"https://localhost:{port}"), handler, TimeSpan.FromSeconds(5));
            bool ready = false;
            string lastFailure = "";
            for (int i = 0; i < 50; i++)
            {
                if (process.HasExited)
                    throw new InvalidOperationException("Kestrel stopped: " + await stderr + await stdout);
                try
                {
                    if ((await health.GetAsync($"https://localhost:{port}/health")).IsSuccessStatusCode)
                    {
                        ready = true;
                        break;
                    }
                }
                catch (HttpRequestException ex)
                {
                    lastFailure = ex.ToString();
                    await Task.Delay(100);
                }
            }

            if (!ready)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
                throw new InvalidOperationException("Kestrel HTTPS did not become available. " + lastFailure + await stdout + await stderr);
            }

            Assert.NotNull(await client.LoginAsync("tlsstudent", "Test-password-123"));
            await client.CallAsync("Study.LogSessionAsync", [studentId, 0, 60, "Real HTTPS -> EF -> SQL Server"]);
            Assert.Equal(60, await client.CallAsync<int>("Study.GetTotalMinutesAsync", [studentId]));
            await using (var db = test.Open())
                Assert.Equal(60, await new StudyRepository(db).GetTotalMinutesAsync(studentId));
            await client.LogoutAsync();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => client.CallAsync("Study.GetTotalMinutesAsync", [studentId]));
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            await stdout;
            await stderr;
            File.Delete(artifact);
        }
    }
}
