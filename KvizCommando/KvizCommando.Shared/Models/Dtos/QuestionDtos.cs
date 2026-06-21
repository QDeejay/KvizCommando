


namespace KvizCommando.Shared.Models.Dtos
{
    public class QuestionDtos
    {
        public bool AccessDenied { get; set; } = false;
        public int[] FactorySlots { get; set; } = new int[11]; // Fill up category names no

        public UserSlot[] Userlots { get; set; } = new UserSlot[11]; // Userdeclareted questions

        public PendingSlot[] PendingSlots { get; set; } = new PendingSlot[6];

        public QuestionExtendedInfo ExtendedInfo { get; set; } = new();

        //public QuestionButtons QuestionButtons { get; set; } = new();
    }
    public class QuestionExtendedInfo
    {
        public int AvailablePendingSlot { get; set; } = 0;
        public int AvailableUserSlot { get; set; } = 0;
        public int FreeUserSlot { get; set; } = 0;
        public int FreePendingSlot { get; set; } = 0;

        public int OccupiedUserSlot { get; set; } = 0;
        public int OccupiedPendingSlot { get; set; } = 0;

        public int HandlePendingSlot { get; set; } = 0;

        public int NoFownQuestion { get; set; } = 0;
        public bool UserSlotEnable { get; set; } = false;

        public bool[] CharCatMask { get; set; } = new bool[8];

    }
    public class UserSlot
    {

        public string Question { get; set; } = string.Empty;
        public int Category { get; set; } = 0;

        public string Ratio { get; set; } = string.Empty;
    }
    public class PendingSlot : UserSlot
    {

        public string Status { get; set; } = string.Empty;

        public string? Remark { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }

    /*
    //public class QuestionButtons
    {
        public bool btnFactorySave { get; set; } = false;
        public bool btnUsrSave { get; set; } = false;
        public bool btnUsrDelete { get; set; } = false;
        public bool btnPendingSendApr { get; set; } = false;
        public bool btnPendigMoveUsr { get; set; } = false;
        public bool btnPendingDelete { get; set; } = false;


    }
    */
}
