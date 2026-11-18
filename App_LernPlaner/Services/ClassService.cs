using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App_LernPlaner.Data;
using App_LernPlaner.Models;
using App_LernPlaner.Repositories;

namespace App_LernPlaner.Services
{
    /// <summary>Klassen-System: erstellen, beitreten, verwalten (FA-07 bis FA-11).</summary>
    public class ClassService
    {
        private const byte TeacherRole = 1;
        private const byte StudentRole = 2;

        private readonly ClassRepository _classes = new();

        public async Task<List<Class>> GetClassesForUserAsync(int userId)
        {
            if (AppConfig.UseMockData)
            {
                var classIds = MockDatabase.ClassMembers.Where(cm => cm.UserId == userId).Select(cm => cm.ClassId).ToHashSet();
                return MockDatabase.Classes.Where(c => classIds.Contains(c.ClassId)).ToList();
            }

            return await _classes.GetForUserAsync(userId);
        }

        public async Task<List<Class>> GetAllClassesAsync()
        {
            if (AppConfig.UseMockData) return MockDatabase.Classes.ToList();
            return await _classes.GetAllAsync();
        }

        public async Task<Class?> GetByIdAsync(int classId)
        {
            if (AppConfig.UseMockData) return MockDatabase.Classes.FirstOrDefault(c => c.ClassId == classId);
            return await _classes.GetByIdAsync(classId);
        }

        public async Task<List<User>> GetMembersAsync(int classId, byte? role = null)
        {
            if (AppConfig.UseMockData)
            {
                var memberIds = MockDatabase.ClassMembers
                    .Where(cm => cm.ClassId == classId && (role == null || cm.Role == role))
                    .Select(cm => cm.UserId).ToHashSet();
                return MockDatabase.Users.Where(u => memberIds.Contains(u.UserId)).ToList();
            }

            return await _classes.GetMembersAsync(classId, role);
        }

        public async Task<int> CountStudentsAsync(int classId)
        {
            if (AppConfig.UseMockData) return (await GetMembersAsync(classId, StudentRole)).Count;
            return await _classes.CountStudentsAsync(classId);
        }

        // FA-07 / FA-08: Lehrer erstellt Klasse mit eindeutigem Einladungscode
        public async Task<Class> CreateClassAsync(string name, int teacherId)
        {
            name = name.Trim();

            if (AppConfig.UseMockData)
            {
                var cls = new Class { ClassId = MockDatabase.NextClassId(), Name = name, TeacherId = teacherId, JoinCode = MockDatabase.GenerateJoinCode(), CreatedAt = DateTime.Now };
                MockDatabase.Classes.Add(cls);
                MockDatabase.ClassMembers.Add(new ClassMember { ClassId = cls.ClassId, UserId = teacherId, Role = TeacherRole, JoinedAt = DateTime.Now });
                return cls;
            }

            string joinCode = await GenerateUniqueJoinCodeAsync();
            return await _classes.CreateAsync(name, teacherId, joinCode);
        }

        private async Task<string> GenerateUniqueJoinCodeAsync()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            for (int attempt = 0; attempt < 20; attempt++)
            {
                string code = new string(Enumerable.Range(0, 7).Select(_ => chars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
                if (await _classes.GetByJoinCodeAsync(code) == null) return code;
            }
            throw new InvalidOperationException("Konnte keinen eindeutigen Einladungscode erzeugen.");
        }

        // FA-09: Schüler tritt per Code bei
        public async Task<(bool Success, string Error)> TryJoinClassAsync(string joinCode, int userId)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current is { } remote)
            {
                await remote.PostAsync<bool>("classes/join",new LernPlaner.Core.JoinRequest(joinCode,userId));
                return (true, string.Empty);
            }
            joinCode = joinCode.Trim();

            if (AppConfig.UseMockData)
            {
                var cls = MockDatabase.Classes.FirstOrDefault(c => string.Equals(c.JoinCode, joinCode, StringComparison.OrdinalIgnoreCase));
                if (cls == null) return (false, "Keine Klasse mit diesem Einladungscode gefunden.");
                if (MockDatabase.ClassMembers.Any(cm => cm.ClassId == cls.ClassId && cm.UserId == userId))
                    return (false, "Du bist dieser Klasse bereits beigetreten.");

                MockDatabase.ClassMembers.Add(new ClassMember { ClassId = cls.ClassId, UserId = userId, Role = StudentRole, JoinedAt = DateTime.Now });
                return (true, string.Empty);
            }

            var dbClass = await _classes.GetByJoinCodeAsync(joinCode);
            if (dbClass == null) return (false, "Keine Klasse mit diesem Einladungscode gefunden.");

            if (await _classes.IsMemberAsync(dbClass.ClassId, userId))
                return (false, "Du bist dieser Klasse bereits beigetreten.");

            await _classes.AddMemberAsync(dbClass.ClassId, userId, StudentRole);
            return (true, string.Empty);
        }

        // Lehrer fügt einen Schüler direkt hinzu (Admin/Lehrer-Verwaltung)
        public async Task<(bool Success, string Error)> AddStudentAsync(int classId, int studentUserId)
        {
            if (AppConfig.UseMockData)
            {
                if (MockDatabase.ClassMembers.Any(cm => cm.ClassId == classId && cm.UserId == studentUserId))
                    return (false, "Schüler ist bereits Mitglied dieser Klasse.");
                MockDatabase.ClassMembers.Add(new ClassMember { ClassId = classId, UserId = studentUserId, Role = StudentRole, JoinedAt = DateTime.Now });
                return (true, string.Empty);
            }

            if (await _classes.IsMemberAsync(classId, studentUserId))
                return (false, "Schüler ist bereits Mitglied dieser Klasse.");

            await _classes.AddMemberAsync(classId, studentUserId, StudentRole);
            return (true, string.Empty);
        }

        // FA-10: Lehrer entfernt Schüler aus Klasse
        public async Task RemoveMemberAsync(int classId, int userId)
        {
            if (AppConfig.UseMockData)
            {
                MockDatabase.ClassMembers.RemoveAll(cm => cm.ClassId == classId && cm.UserId == userId);
                return;
            }
            await _classes.RemoveMemberAsync(classId, userId);
        }

        // FA-11: Klasse archivieren
        public async Task DeleteClassAsync(int classId)
        {
            if (AppConfig.UseMockData)
            {
                MockDatabase.Classes.RemoveAll(c => c.ClassId == classId);
                MockDatabase.ClassMembers.RemoveAll(cm => cm.ClassId == classId);
                return;
            }
            await _classes.DeleteAsync(classId);
        }

        public async Task<bool> IsTeacherOfAsync(int classId, int userId)
        {
            var members = await GetMembersAsync(classId, TeacherRole);
            return members.Any(u => u.UserId == userId);
        }
    }
}
