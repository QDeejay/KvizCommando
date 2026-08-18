using System.Collections.Generic;


namespace KvizCommando.Server.Models.ForAdminClient
{
    public class FactoryResponse
    {
        public string Message { get; set; }
        public List<ModelQuestions> Questions { get; set; }
    }
}
