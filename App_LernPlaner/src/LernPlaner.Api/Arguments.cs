using System.Text.Json;

namespace LernPlaner.Api;
public sealed class Arguments
{
    private readonly Dictionary<string, JsonElement> values = new();
    public Arguments(string operation, JsonElement[] args)
    {
        string[] names = operation switch
        {
            "User.FindByUsernameAsync" => new string[]
            {
                "username"
            },
            "User.FindByEmailAsync" => new string[]
            {
                "email"
            },
            "User.GetByIdAsync" => new string[]
            {
                "userId"
            },
            "User.GetAllAsync" => new string[]
            {
            },
            "User.ExistsAsync" => new string[]
            {
                "username",
                "email"
            },
            "User.InsertAsync" => new string[]
            {
                "username",
                "passwordHash",
                "email",
                "role"
            },
            "User.UpdatePasswordAsync" => new string[]
            {
                "userId",
                "newHash"
            },
            "User.UpdateRoleAsync" => new string[]
            {
                "userId",
                "role"
            },
            "User.UpdateActiveAsync" => new string[]
            {
                "userId",
                "isActive"
            },
            "User.UpdateProfileAsync" => new string[]
            {
                "userId",
                "firstName",
                "lastName",
                "profilePicturePath"
            },
            "User.AddXpAsync" => new string[]
            {
                "userId",
                "amount"
            },
            "User.UpdateUserProfileAsync" => new string[]
            {
                "profile"
            },
            "User.GetUserProfileByIdAsync" => new string[]
            {
                "userId"
            },
            "Class.GetAllAsync" => new string[]
            {
            },
            "Class.GetByIdAsync" => new string[]
            {
                "classId"
            },
            "Class.GetByJoinCodeAsync" => new string[]
            {
                "joinCode"
            },
            "Class.GetForUserAsync" => new string[]
            {
                "userId"
            },
            "Class.GetMembersAsync" => new string[]
            {
                "classId",
                "role"
            },
            "Class.CountStudentsAsync" => new string[]
            {
                "classId"
            },
            "Class.CreateAsync" => new string[]
            {
                "name",
                "teacherId",
                "joinCode"
            },
            "Class.IsMemberAsync" => new string[]
            {
                "classId",
                "userId"
            },
            "Class.AddMemberAsync" => new string[]
            {
                "classId",
                "userId",
                "role"
            },
            "Class.RemoveMemberAsync" => new string[]
            {
                "classId",
                "userId"
            },
            "Class.DeleteAsync" => new string[]
            {
                "classId"
            },
            "Task.GetAllTasks" => new string[]
            {
            },
            "Task.GetTasksByUserIdAsync" => new string[]
            {
                "UserId"
            },
            "Task.GetByIdAsync" => new string[]
            {
                "taskId"
            },
            "Task.GetVisibleTasksForUserAsync" => new string[]
            {
                "userId"
            },
            "Task.GetTasksByClassIdAsync" => new string[]
            {
                "classId"
            },
            "Task.UpdateAsync" => new string[]
            {
                "task"
            },
            "Task.HinzufugenTask" => new string[]
            {
                "task"
            },
            "Task.StatusAendernAsync" => new string[]
            {
                "taskId",
                "neuerStatus"
            },
            "Task.LoeschenAsync" => new string[]
            {
                "taskId"
            },
            "Submission.SaveProgressAsync" => new string[]
            {
                "submission"
            },
            "Submission.GetByTaskAndUserAsync" => new string[]
            {
                "taskId",
                "userId"
            },
            "Submission.GetForTaskAsync" => new string[]
            {
                "taskId"
            },
            "Submission.GetForUserAsync" => new string[]
            {
                "userId"
            },
            "Submission.GradeAsync" => new string[]
            {
                "submissionId",
                "grade",
                "feedback"
            },
            "Study.LogSessionAsync" => new string[]
            {
                "userId",
                "taskId",
                "durationMin",
                "notes"
            },
            "Study.GetForUserAsync" => new string[]
            {
                "userId"
            },
            "Study.GetTotalMinutesAsync" => new string[]
            {
                "userId"
            },
            "Notification.GetForUserAsync" => new string[]
            {
                "userId"
            },
            "Notification.CountUnreadAsync" => new string[]
            {
                "userId"
            },
            "Notification.InsertAsync" => new string[]
            {
                "userId",
                "message"
            },
            "Notification.ExistsSimilarAsync" => new string[]
            {
                "userId",
                "messagePrefix"
            },
            "Notification.MarkAsReadAsync" => new string[]
            {
                "notifId"
            },
            "Notification.MarkAllAsReadAsync" => new string[]
            {
                "userId"
            },
            "Gamification.GetAllBadgesAsync" => new string[]
            {
            },
            "Gamification.GetUserBadgesAsync" => new string[]
            {
                "userId"
            },
            "Gamification.HasBadgeAsync" => new string[]
            {
                "userId",
                "badgeId"
            },
            "Gamification.GetBadgeIdByNameAsync" => new string[]
            {
                "name"
            },
            "Gamification.AwardBadgeAsync" => new string[]
            {
                "userId",
                "badgeId"
            },
            "Gamification.GetLeaderboardAsync" => new string[]
            {
                "classId"
            },
            "Message.GetConversationAsync" => new string[]
            {
                "userA",
                "userB"
            },
            "Message.GetContactsWithLastMessageAsync" => new string[]
            {
                "userId"
            },
            "Message.CountUnreadTotalAsync" => new string[]
            {
                "userId"
            },
            "Message.SendAsync" => new string[]
            {
                "senderId",
                "receiverId",
                "body"
            },
            "Message.EditAsync" => new string[]
            {
                "messageId",
                "senderId",
                "newBody"
            },
            "Message.SoftDeleteAsync" => new string[]
            {
                "messageId",
                "senderId"
            },
            "Message.MarkConversationReadAsync" => new string[]
            {
                "userId",
                "otherUserId"
            },
            "Message.GetInboxAsync" => new string[]
            {
                "userId"
            },
            _ => throw new ArgumentException("Unknown operation.")};
        if (args == null || args.Length != names.Length)
            throw new ArgumentException("Invalid arguments.");
        for (int i = 0; i < names.Length; i++)
        {
            if (args[i].ValueKind == JsonValueKind.Null && names[i] != "role" && names[i] != "profilePicturePath")
                throw new ArgumentException("Null argument.");
            values[names[i]] = args[i];
        }
    }

    public T Get<T>(string name)
    {
        if (!values.TryGetValue(name, out var v))
            throw new ArgumentException("Missing argument.");
        try
        {
            return v.Deserialize<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        }
        catch (JsonException)
        {
            throw new ArgumentException("Invalid argument type.");
        }
    }
}
