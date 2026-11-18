using App_LernPlaner.Models;
using LernPlaner.Core;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Api;
/// <summary>Explicit allowlist. Identity and current roles are loaded from SQL Server on every request.</summary>
public sealed class OperationPolicy(LernPlanerDbContext db, User actor)
{
    public async Task DemandAsync(string op, Arguments a, CancellationToken ct)
    {
        var admin = actor.IsAdmin;
        var id = actor.UserId;
        void Require(bool allowed)
        {
            if (!allowed)
                throw new ForbiddenException();
        }

        async Task<bool> Member(int c) => admin || await db.ClassMembers.AnyAsync(x => x.ClassId == c && x.UserId == id, ct);
        async Task<bool> Manage(int c) => admin || actor.IsTeacher && await db.Classes.AnyAsync(x => x.ClassId == c && x.TeacherId == id, ct);
        async Task<StudyTask> Task(int t) => await db.Tasks.AsNoTracking().SingleOrDefaultAsync(x => x.TaskId == t, ct) ?? throw new ForbiddenException();
        async Task<bool> SeeTask(int t)
        {
            var task = await Task(t);
            return admin || task.UserId == id || task.CreatedBy == id || task.ClassId.HasValue && await Member(task.ClassId.Value);
        }

        async Task<bool> EditTask(int t)
        {
            var task = await Task(t);
            return admin || task.CreatedBy == id && (!task.ClassId.HasValue || await Manage(task.ClassId.Value));
        }

        async Task<bool> ReviewTask(int t)
        {
            var task = await Task(t);
            return admin || actor.IsTeacher && (task.CreatedBy == id || task.ClassId.HasValue && await Manage(task.ClassId.Value));
        }

        switch (op)
        {
            case "User.FindByUsernameAsync":
            case "User.FindByEmailAsync":
            case "User.ExistsAsync":
            case "User.InsertAsync":
            case "User.UpdatePasswordAsync":
            case "User.UpdateRoleAsync":
            case "User.UpdateActiveAsync":
                Require(admin);
                break;
            case "User.GetAllAsync":
                Require(admin || actor.IsTeacher);
                break;
            case "User.GetByIdAsync":
                break; // Public directory projection is applied to other users.
            case "User.GetUserProfileByIdAsync":
                Require(admin || a.Get<int>("userId") == id);
                break;
            case "User.UpdateProfileAsync":
                Require(admin || a.Get<int>("userId") == id);
                break;
            case "User.UpdateUserProfileAsync":
                Require(admin || a.Get<UserProfile>("profile").UserId == id);
                break;
            case "User.AddXpAsync":
            case "Gamification.AwardBadgeAsync":
                throw new ForbiddenException();
            case "Class.GetAllAsync":
                break; // Scoped result is applied before dispatch.
            case "Class.GetByIdAsync":
            case "Class.GetMembersAsync":
            case "Class.CountStudentsAsync":
                Require(await Member(a.Get<int>("classId")));
                break;
            case "Class.GetByJoinCodeAsync":
                Require(admin || actor.IsTeacher);
                break;
            case "Class.GetForUserAsync":
                Require(admin || a.Get<int>("userId") == id);
                break;
            case "Class.CreateAsync":
                Require((admin || actor.IsTeacher) && (admin || a.Get<int>("teacherId") == id));
                Require(await db.Users.AnyAsync(x => x.UserId == a.Get<int>("teacherId") && (x.Role == Roles.Lehrer || x.Role == Roles.Admin), ct));
                break;
            case "Class.IsMemberAsync":
                Require(admin || a.Get<int>("userId") == id || await Manage(a.Get<int>("classId")));
                break;
            case "Class.AddMemberAsync":
                Require(await Manage(a.Get<int>("classId")));
                Require(a.Get<byte>("role") == 2);
                Require(await db.Users.AnyAsync(x => x.UserId == a.Get<int>("userId") && x.Role == Roles.Schueler, ct));
                break;
            case "Class.RemoveMemberAsync":
                Require(await Manage(a.Get<int>("classId")));
                Require(!await db.Classes.AnyAsync(x => x.ClassId == a.Get<int>("classId") && x.TeacherId == a.Get<int>("userId"), ct));
                break;
            case "Class.DeleteAsync":
                Require(await Manage(a.Get<int>("classId")));
                break;
            case "Task.GetAllTasks":
                break; // Scoped result is applied before dispatch.
            case "Task.GetTasksByUserIdAsync":
                Require(admin || a.Get<int>("UserId") == id);
                break;
            case "Task.GetVisibleTasksForUserAsync":
                Require(admin || a.Get<int>("userId") == id);
                break;
            case "Task.GetByIdAsync":
                Require(await SeeTask(a.Get<int>("taskId")));
                break;
            case "Task.GetTasksByClassIdAsync":
                Require(await Member(a.Get<int>("classId")));
                break;
            case "Task.HinzufugenTask":
                var t = a.Get<StudyTask>("task");
                Require(t.TaskId == 0 && t.UserId == id && t.CreatedBy == id);
                Require(!t.ClassId.HasValue || await Manage(t.ClassId.Value));
                break;
            case "Task.UpdateAsync":
                var edit = a.Get<StudyTask>("task");
                Require(await EditTask(edit.TaskId));
                Require(!edit.ClassId.HasValue || await Manage(edit.ClassId.Value));
                break;
            case "Task.LoeschenAsync":
            case "Task.StatusAendernAsync":
                Require(await EditTask(a.Get<int>("taskId")));
                break;
            case "Submission.SaveProgressAsync":
                var s = a.Get<Submission>("submission");
                Require(s.UserId == id && await SeeTask(s.TaskId));
                Require(s.Grade == null && s.Feedback == null);
                break;
            case "Submission.GetByTaskAndUserAsync":
                Require(await SeeTask(a.Get<int>("taskId")));
                Require(a.Get<int>("userId") == id || await ReviewTask(a.Get<int>("taskId")));
                break;
            case "Submission.GetForTaskAsync":
                Require(await ReviewTask(a.Get<int>("taskId")));
                break;
            case "Submission.GetForUserAsync":
                var target = a.Get<int>("userId");
                Require(admin || target == id || actor.IsTeacher && await db.ClassMembers.AnyAsync(m => m.UserId == target && db.Classes.Any(c => c.ClassId == m.ClassId && c.TeacherId == id), ct));
                break;
            case "Submission.GradeAsync":
                var sub = await db.Submissions.AsNoTracking().SingleOrDefaultAsync(x => x.SubmissionId == a.Get<int>("submissionId"), ct);
                Require(sub != null && sub.Status >= 2 && await ReviewTask(sub.TaskId));
                break;
            case "Study.LogSessionAsync":
                Require(a.Get<int>("userId") == id);
                var taskId = a.Get<int>("taskId");
                Require(taskId == 0 || await SeeTask(taskId));
                break;
            case "Study.GetForUserAsync":
            case "Study.GetTotalMinutesAsync":
                Require(admin || a.Get<int>("userId") == id);
                break;
            case "Notification.GetForUserAsync":
            case "Notification.CountUnreadAsync":
            case "Notification.ExistsSimilarAsync":
            case "Notification.MarkAllAsReadAsync":
                Require(a.Get<int>("userId") == id);
                break;
            case "Notification.InsertAsync":
                Require(a.Get<int>("userId") == id);
                Require(a.Get<string>("message") != "__daily_login__");
                break;
            case "Notification.MarkAsReadAsync":
                Require(await db.Notifications.AnyAsync(x => x.NotifId == a.Get<int>("notifId") && x.UserId == id, ct));
                break;
            case "Gamification.GetAllBadgesAsync":
            case "Gamification.GetBadgeIdByNameAsync":
                break;
            case "Gamification.GetUserBadgesAsync":
            case "Gamification.HasBadgeAsync":
                Require(admin || a.Get<int>("userId") == id);
                break;
            case "Gamification.GetLeaderboardAsync":
                Require(await Member(a.Get<int>("classId")));
                break;
            case "Message.GetConversationAsync":
                Require(a.Get<int>("userA") == id || a.Get<int>("userB") == id);
                break;
            case "Message.GetContactsWithLastMessageAsync":
            case "Message.CountUnreadTotalAsync":
            case "Message.MarkConversationReadAsync":
            case "Message.GetInboxAsync":
                Require(a.Get<int>("userId") == id);
                break;
            case "Message.SendAsync":
                Require(a.Get<int>("senderId") == id);
                Require(await db.Users.AnyAsync(x => x.UserId == a.Get<int>("receiverId") && x.IsActive, ct));
                break;
            case "Message.EditAsync":
            case "Message.SoftDeleteAsync":
                Require(a.Get<int>("senderId") == id);
                Require(await db.Messages.AnyAsync(x => x.MessageId == a.Get<int>("messageId") && x.SenderId == id, ct));
                break;
            default:
                throw new ForbiddenException();
        }
    }
}

public sealed class ForbiddenException : Exception;
