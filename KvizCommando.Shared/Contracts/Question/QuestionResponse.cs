using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Question
{
    public sealed class QuestionResponse
    {
        public bool Success { get; init; }
        public List<string> Errors { get; init; } = new();
    }
}
