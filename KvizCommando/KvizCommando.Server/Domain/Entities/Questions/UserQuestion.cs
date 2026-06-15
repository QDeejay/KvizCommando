namespace KvizCommando.Server.Domain.Entities.Questions
{
    public class UserQuestion : QuizQuestion
    {
        public int Ask { get; set; } = 0;

        public int OkAnswer { get; set; } = 0;

        public double Ratio { get; private set; } = 0.0;
    }
}
