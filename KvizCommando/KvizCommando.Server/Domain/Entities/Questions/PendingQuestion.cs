using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Server.Domain.Entities.Questions
{
    public class PendingQuestion : QuizQuestion

    {

        public QuestionStatus Status { get; set; } = QuestionStatus.None;

        public string? Remark { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
