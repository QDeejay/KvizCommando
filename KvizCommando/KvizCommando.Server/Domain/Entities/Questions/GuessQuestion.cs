namespace KvizCommando.Server.Domain.Entities.Questions
{
    public class GuessQuestion
    {
        public int Id { get; set; }

        public string Question { get; set; } = string.Empty;

        public double Answer { get; set; } = 0;

        public int Reported { get; set; } = 0; // Jelölés problémás kérdés esetén
    }
}
