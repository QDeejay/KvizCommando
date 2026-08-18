using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Question
{
    public sealed class SaveFactoryRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public int[] CategorySlots { get; set; } = Array.Empty<int>();
    }
}
