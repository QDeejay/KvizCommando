using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Question
{
    public sealed class SaveFactoryRequest
    {
        // Pl. 12 elemű lista; a szerver oldali validáció kezeli a szabályokat.
        public int[] CategorySlots { get; set; } = Array.Empty<int>();
    }
}
