using App_LernPlaner.Models;
using System.Net.Mail;

namespace LernPlaner.Core;
public static class Input
{
    public static void Text(string? value, int max, bool empty = false)
    {
        if (value == null || (!empty && string.IsNullOrWhiteSpace(value)) || value.Length > max)
            throw new ArgumentException("Invalid text length.");
    }

    public static void Email(string value)
    {
        Text(value, 254);
        if (!MailAddress.TryCreate(value, out var a) || a.Address != value)
            throw new ArgumentException("Invalid email.");
    }

    public static void Role(string role)
    {
        if (role != Roles.Admin && role != Roles.Lehrer && role != Roles.Schueler)
            throw new ArgumentException("Invalid role.");
    }

    public static void Hash(string hash)
    {
        if (hash == null || !System.Text.RegularExpressions.Regex.IsMatch(hash, @"^\$2[aby]\$(1[0-4])\$[./A-Za-z0-9]{53}$"))
            throw new ArgumentException("Invalid password hash.");
    }

    public static void Password(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || System.Text.Encoding.UTF8.GetByteCount(password) > 72)
            throw new ArgumentException("Password must contain 8-72 UTF-8 bytes.");
    }

    public static void Task(StudyTask t)
    {
        Text(t.Titel, 300);
        Text(t.Subject, 150);
        Text(t.Description, 10000, true);
        if (t.Priority < 1 || t.Priority > 3 || t.Status > 2 || t.Deadline == default)
            throw new ArgumentException("Invalid task.");
    }

    public static void Submission(Submission s)
    {
        Text(s.Notizen, 10000, true);
        Text(s.Abgabe, 10000, true);
        Text(s.FilePath, 1024, true);
        if (s.Status > 2 || s.LearningMinutes < 0 || s.LearningMinutes > 1440)
            throw new ArgumentException("Invalid submission.");
    }
}

public sealed record LoginRequest(string Username, string Password);
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt, User User);
public sealed record PasswordRequest(int UserId, string CurrentPassword, string NewPassword);
public sealed record RegisterRequest(string Username, string Password, string Email, string Role);
public sealed record JoinRequest(string JoinCode, int UserId);
