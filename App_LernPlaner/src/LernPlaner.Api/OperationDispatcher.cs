using LernPlaner.Core;
using LernPlaner.Core.Repositories;
using App_LernPlaner.Models;

namespace LernPlaner.Api;
public static class OperationDispatcher
{
    public static async Task<object?> ExecuteAsync(string operation, Arguments a, LernPlanerDbContext db, CancellationToken ct)
    {
        switch (operation)
        {
            case "User.FindByUsernameAsync":
                return await new UserRepository(db).FindByUsernameAsync(a.Get<string>("username"), ct);
            case "User.FindByEmailAsync":
                return await new UserRepository(db).FindByEmailAsync(a.Get<string>("email"), ct);
            case "User.GetByIdAsync":
                return await new UserRepository(db).GetByIdAsync(a.Get<int>("userId"), ct);
            case "User.GetAllAsync":
                return await new UserRepository(db).GetAllAsync(ct);
            case "User.ExistsAsync":
                return await new UserRepository(db).ExistsAsync(a.Get<string>("username"), a.Get<string>("email"), ct);
            case "User.InsertAsync":
                return await new UserRepository(db).InsertAsync(a.Get<string>("username"), a.Get<string>("passwordHash"), a.Get<string>("email"), a.Get<string>("role"), ct);
            case "User.UpdatePasswordAsync":
                await new UserRepository(db).UpdatePasswordAsync(a.Get<int>("userId"), a.Get<string>("newHash"), ct);
                return null;
            case "User.UpdateRoleAsync":
                await new UserRepository(db).UpdateRoleAsync(a.Get<int>("userId"), a.Get<string>("role"), ct);
                return null;
            case "User.UpdateActiveAsync":
                await new UserRepository(db).UpdateActiveAsync(a.Get<int>("userId"), a.Get<bool>("isActive"), ct);
                return null;
            case "User.UpdateProfileAsync":
                await new UserRepository(db).UpdateProfileAsync(a.Get<int>("userId"), a.Get<string>("firstName"), a.Get<string>("lastName"), a.Get<string?>("profilePicturePath"), ct);
                return null;
            case "User.AddXpAsync":
                await new UserRepository(db).AddXpAsync(a.Get<int>("userId"), a.Get<int>("amount"), ct);
                return null;
            case "User.UpdateUserProfileAsync":
                await new UserRepository(db).UpdateUserProfileAsync(a.Get<UserProfile>("profile"), ct);
                return null;
            case "User.GetUserProfileByIdAsync":
                return await new UserRepository(db).GetUserProfileByIdAsync(a.Get<int>("userId"), ct);
            case "Class.GetAllAsync":
                return await new ClassRepository(db).GetAllAsync(ct);
            case "Class.GetByIdAsync":
                return await new ClassRepository(db).GetByIdAsync(a.Get<int>("classId"), ct);
            case "Class.GetByJoinCodeAsync":
                return await new ClassRepository(db).GetByJoinCodeAsync(a.Get<string>("joinCode"), ct);
            case "Class.GetForUserAsync":
                return await new ClassRepository(db).GetForUserAsync(a.Get<int>("userId"), ct);
            case "Class.GetMembersAsync":
                return await new ClassRepository(db).GetMembersAsync(a.Get<int>("classId"), a.Get<byte?>("role"), ct);
            case "Class.CountStudentsAsync":
                return await new ClassRepository(db).CountStudentsAsync(a.Get<int>("classId"), ct);
            case "Class.CreateAsync":
                return await new ClassRepository(db).CreateAsync(a.Get<string>("name"), a.Get<int>("teacherId"), a.Get<string>("joinCode"), ct);
            case "Class.IsMemberAsync":
                return await new ClassRepository(db).IsMemberAsync(a.Get<int>("classId"), a.Get<int>("userId"), ct);
            case "Class.AddMemberAsync":
                await new ClassRepository(db).AddMemberAsync(a.Get<int>("classId"), a.Get<int>("userId"), a.Get<byte>("role"), ct);
                return null;
            case "Class.RemoveMemberAsync":
                await new ClassRepository(db).RemoveMemberAsync(a.Get<int>("classId"), a.Get<int>("userId"), ct);
                return null;
            case "Class.DeleteAsync":
                await new ClassRepository(db).DeleteAsync(a.Get<int>("classId"), ct);
                return null;
            case "Task.GetAllTasks":
                return await new TaskRepository(db).GetAllTasks(ct);
            case "Task.GetTasksByUserIdAsync":
                return await new TaskRepository(db).GetTasksByUserIdAsync(a.Get<int>("UserId"), ct);
            case "Task.GetByIdAsync":
                return await new TaskRepository(db).GetByIdAsync(a.Get<int>("taskId"), ct);
            case "Task.GetVisibleTasksForUserAsync":
                return await new TaskRepository(db).GetVisibleTasksForUserAsync(a.Get<int>("userId"), ct);
            case "Task.GetTasksByClassIdAsync":
                return await new TaskRepository(db).GetTasksByClassIdAsync(a.Get<int>("classId"), ct);
            case "Task.UpdateAsync":
                await new TaskRepository(db).UpdateAsync(a.Get<StudyTask>("task"), ct);
                return null;
            case "Task.HinzufugenTask":
                await new TaskRepository(db).HinzufugenTask(a.Get<StudyTask>("task"), ct);
                return null;
            case "Task.StatusAendernAsync":
                await new TaskRepository(db).StatusAendernAsync(a.Get<int>("taskId"), a.Get<byte>("neuerStatus"), ct);
                return null;
            case "Task.LoeschenAsync":
                await new TaskRepository(db).LoeschenAsync(a.Get<int>("taskId"), ct);
                return null;
            case "Submission.SaveProgressAsync":
                await new SubmissionRepository(db).SaveProgressAsync(a.Get<Submission>("submission"), ct);
                return null;
            case "Submission.GetByTaskAndUserAsync":
                return await new SubmissionRepository(db).GetByTaskAndUserAsync(a.Get<int>("taskId"), a.Get<int>("userId"), ct);
            case "Submission.GetForTaskAsync":
                return await new SubmissionRepository(db).GetForTaskAsync(a.Get<int>("taskId"), ct);
            case "Submission.GetForUserAsync":
                return await new SubmissionRepository(db).GetForUserAsync(a.Get<int>("userId"), ct);
            case "Submission.GradeAsync":
                await new SubmissionRepository(db).GradeAsync(a.Get<int>("submissionId"), a.Get<string>("grade"), a.Get<string>("feedback"), ct);
                return null;
            case "Study.LogSessionAsync":
                await new StudyRepository(db).LogSessionAsync(a.Get<int>("userId"), a.Get<int>("taskId"), a.Get<int>("durationMin"), a.Get<string>("notes"), ct);
                return null;
            case "Study.GetForUserAsync":
                return await new StudyRepository(db).GetForUserAsync(a.Get<int>("userId"), ct);
            case "Study.GetTotalMinutesAsync":
                return await new StudyRepository(db).GetTotalMinutesAsync(a.Get<int>("userId"), ct);
            case "Notification.GetForUserAsync":
                return await new NotificationRepository(db).GetForUserAsync(a.Get<int>("userId"), ct);
            case "Notification.CountUnreadAsync":
                return await new NotificationRepository(db).CountUnreadAsync(a.Get<int>("userId"), ct);
            case "Notification.InsertAsync":
                await new NotificationRepository(db).InsertAsync(a.Get<int>("userId"), a.Get<string>("message"), ct);
                return null;
            case "Notification.ExistsSimilarAsync":
                return await new NotificationRepository(db).ExistsSimilarAsync(a.Get<int>("userId"), a.Get<string>("messagePrefix"), ct);
            case "Notification.MarkAsReadAsync":
                await new NotificationRepository(db).MarkAsReadAsync(a.Get<int>("notifId"), ct);
                return null;
            case "Notification.MarkAllAsReadAsync":
                await new NotificationRepository(db).MarkAllAsReadAsync(a.Get<int>("userId"), ct);
                return null;
            case "Gamification.GetAllBadgesAsync":
                return await new GamificationRepository(db).GetAllBadgesAsync(ct);
            case "Gamification.GetUserBadgesAsync":
                return await new GamificationRepository(db).GetUserBadgesAsync(a.Get<int>("userId"), ct);
            case "Gamification.HasBadgeAsync":
                return await new GamificationRepository(db).HasBadgeAsync(a.Get<int>("userId"), a.Get<int>("badgeId"), ct);
            case "Gamification.GetBadgeIdByNameAsync":
                return await new GamificationRepository(db).GetBadgeIdByNameAsync(a.Get<string>("name"), ct);
            case "Gamification.AwardBadgeAsync":
                await new GamificationRepository(db).AwardBadgeAsync(a.Get<int>("userId"), a.Get<int>("badgeId"), ct);
                return null;
            case "Gamification.GetLeaderboardAsync":
                return await new GamificationRepository(db).GetLeaderboardAsync(a.Get<int>("classId"), ct);
            case "Message.GetConversationAsync":
                return await new MessageRepository(db).GetConversationAsync(a.Get<int>("userA"), a.Get<int>("userB"), ct);
            case "Message.GetContactsWithLastMessageAsync":
                return await new MessageRepository(db).GetContactsWithLastMessageAsync(a.Get<int>("userId"), ct);
            case "Message.CountUnreadTotalAsync":
                return await new MessageRepository(db).CountUnreadTotalAsync(a.Get<int>("userId"), ct);
            case "Message.SendAsync":
                return await new MessageRepository(db).SendAsync(a.Get<int>("senderId"), a.Get<int>("receiverId"), a.Get<string>("body"), ct);
            case "Message.EditAsync":
                await new MessageRepository(db).EditAsync(a.Get<int>("messageId"), a.Get<int>("senderId"), a.Get<string>("newBody"), ct);
                return null;
            case "Message.SoftDeleteAsync":
                await new MessageRepository(db).SoftDeleteAsync(a.Get<int>("messageId"), a.Get<int>("senderId"), ct);
                return null;
            case "Message.MarkConversationReadAsync":
                await new MessageRepository(db).MarkConversationReadAsync(a.Get<int>("userId"), a.Get<int>("otherUserId"), ct);
                return null;
            case "Message.GetInboxAsync":
                return await new MessageRepository(db).GetInboxAsync(a.Get<int>("userId"), ct);
            default:
                throw new ArgumentException("Unknown operation.");
        }
    }
}
