using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public string Grade { get; set; }
        public string Feedback { get; set; }
        public DateTime SubmittedAt { get; set; }

        public bool IsGraded => !string.IsNullOrEmpty(Grade); 

    }
}
