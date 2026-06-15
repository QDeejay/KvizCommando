namespace KvizCommando.Server.Models.ForAdminClient
{
    public class ModelQuestions
    {
        public int Id { get; set; }
        public int Category { get; set; }
        public string Question { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public string Answer4 { get; set; }
        public bool Status { get; set; }

        public int remoteStatus { get; set; }
        public string remoteFeedBack { get; set; }
    }
}
