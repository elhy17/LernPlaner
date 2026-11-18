using App_LernPlaner.Models;
using Microsoft.EntityFrameworkCore;

namespace LernPlaner.Core.Repositories;
public sealed class SubmissionRepository(LernPlanerDbContext db)
{
    public async Task SaveProgressAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        Input.Submission(submission);
        var s = await db.Submissions.SingleOrDefaultAsync(x => x.TaskId == submission.TaskId && x.UserId == submission.UserId, cancellationToken);
        if (s == null)
        {
            s = new Submission
            {
                TaskId = submission.TaskId,
                UserId = submission.UserId
            };
            db.Submissions.Add(s);
        }

        if (s.IsGraded)
            throw new ArgumentException("Graded submissions cannot be changed.");
        s.Status = submission.Status;
        s.Notizen = submission.Notizen;
        s.Abgabe = submission.Abgabe;
        s.LearningMinutes = submission.LearningMinutes;
        s.FilePath = submission.FilePath;
        s.SubmittedAt = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Submission?> GetByTaskAndUserAsync(int taskId, int userId, CancellationToken cancellationToken = default)
    {
        return await db.Submissions.AsNoTracking().FirstOrDefaultAsync(x => x.TaskId == taskId && x.UserId == userId, cancellationToken);
    }

    public async Task<List<Submission>> GetForTaskAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await db.Submissions.AsNoTracking().Where(x => x.TaskId == taskId).OrderByDescending(x => x.SubmittedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<Submission>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await db.Submissions.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.SubmittedAt).ToListAsync(cancellationToken);
    }

    public async Task GradeAsync(int submissionId, string grade, string feedback, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(grade, out var n) || n < 1 || n > 6)
            throw new ArgumentException("Grade must be 1-6.");
        Input.Text(feedback, 10000, true);
        await db.Submissions.Where(x => x.SubmissionId == submissionId).ExecuteUpdateAsync(s => s.SetProperty(x => x.Grade, grade).SetProperty(x => x.Feedback, feedback).SetProperty(x => x.Status, (byte)3), cancellationToken);
    }
}
