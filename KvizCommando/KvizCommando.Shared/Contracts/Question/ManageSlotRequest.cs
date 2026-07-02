using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Question
{
    public sealed class ManageSlotRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public int SlotNo { get; set; } = 0;
        public SlotManageType ReqType { get; set; }
    }
    public enum SlotManageType
    {
        DeleteUsr =0,
        DeletePending=1,
        MovePending=2,
        SendNew=3,
        Handle=4
    }
}
