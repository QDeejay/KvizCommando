namespace KvizCommando.Server.Domain.Entities.Questions
{
    public abstract class QuizQuestion
    {
        public int Id { get; set; } = 0;

        public int PlayerId { get; set; } = 0;

        public string Question { get; set; } = string.Empty;

        public int CategoryNo { get; set; } = 0;

        public string AnswersJson { get; set; } = string.Empty; // Első az alapértelmezett helyes

        public int Reported { get; set; } = 0; // Jelölés problémás kérdés esetén
    }
}
