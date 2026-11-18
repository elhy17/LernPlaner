using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LernPlaner.Core;

namespace App_LernPlaner.Remote;
/// <summary>Single in-memory session. TLS validation uses the operating system trust store.</summary>
public sealed class RemoteClient : IDisposable
{
    public static RemoteClient? Current { get; private set; }

    private readonly HttpClient http;
    public RemoteClient(Uri address, HttpMessageHandler? handler = null, TimeSpan? timeout = null)
    {
        if (address.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(address.UserInfo) || !string.IsNullOrEmpty(address.Query) || !string.IsNullOrEmpty(address.Fragment))
            throw new ArgumentException("LERNPLANER_API_URL must be an HTTPS base URL without credentials, query or fragment.");
        http = handler == null ? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) : new HttpClient(handler);
        http.BaseAddress = new Uri(address.ToString().TrimEnd('/') + "/");
        http.Timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    public static void Configure(RemoteClient? client)
    {
        Current = client;
    }

    public static void ConfigureFromEnvironment()
    {
        var url = Environment.GetEnvironmentVariable("LERNPLANER_API_URL");
        if (!string.IsNullOrWhiteSpace(url))
            Current = new RemoteClient(new Uri(url));
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        ClearSession();
        using var response = await SendAsync("auth/login", new LoginRequest(username, password), ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return null;
        Ensure(response);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct) ?? throw new InvalidOperationException("Invalid server response.");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        return result;
    }

    public void ClearSession() => http.DefaultRequestHeaders.Authorization = null;
    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            using var r = await SendAsync("auth/logout", new { }, ct);
            Ensure(r);
        }
        finally
        {
            ClearSession();
        }
    }

    public async Task<T> PostAsync<T>(string path, object body, CancellationToken ct = default)
    {
        using var r = await SendAsync(path, body, ct);
        Ensure(r);
        return (await r.Content.ReadFromJsonAsync<T>(cancellationToken: ct))!;
    }

    public async Task<T> CallAsync<T>(string operation, object? [] args, CancellationToken ct = default) => await PostAsync<T>("operations/" + operation, args, ct);
    public async Task CallAsync(string operation, object? [] args, CancellationToken ct = default)
    {
        using var r = await SendAsync("operations/" + operation, args, ct);
        Ensure(r);
    }

    private async Task<HttpResponseMessage> SendAsync(string path, object data, CancellationToken ct)
    {
        try
        {
            return await http.PostAsJsonAsync(path, data, ct);
        }
        catch (OperationCanceledException)when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("Der LernPlaner-Server antwortet nicht rechtzeitig.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Der LernPlaner-Server ist nicht erreichbar. Bitte HTTPS-Adresse und Verbindung prüfen.", ex);
        }
    }

    private void Ensure(HttpResponseMessage r)
    {
        if (r.IsSuccessStatusCode)
            return;
        if (r.StatusCode == HttpStatusCode.Unauthorized)
        {
            ClearSession();
            throw new UnauthorizedAccessException("Sitzung abgelaufen. Bitte erneut anmelden.");
        }

        if (r.StatusCode == HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("Für diese Aktion fehlt die Berechtigung.");
        if (r.StatusCode == HttpStatusCode.BadRequest)
            throw new ArgumentException("Ungültige Eingabe. Bitte Eingaben prüfen.");
        if (r.StatusCode == HttpStatusCode.Conflict)
            throw new InvalidOperationException("Die Daten wurden bereits geändert oder sind bereits vorhanden. Bitte neu laden.");
        throw new InvalidOperationException("Der Server kann die Anfrage momentan nicht bearbeiten.");
    }

    public void Dispose() => http.Dispose();
}
