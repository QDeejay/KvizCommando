namespace KvizCommando.Shared.Contracts.Question
{
    public sealed class NewQuestionRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public int Category { get; set; } = 0;
        public string[] Answers { get; set; } = [string.Empty, string.Empty, string.Empty, string.Empty];
    }
}
