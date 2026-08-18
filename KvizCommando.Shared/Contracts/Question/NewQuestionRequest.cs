using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Question
{
    public sealed class NewQuestionRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public int Category { get; set; } = 0;

        public string[] Answers { get; set; } = [string.Empty,string.Empty,string.Empty,string.Empty];

        public int SlotNo { get; set; } = 0;      
    }
}
