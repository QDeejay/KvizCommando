using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache
{
    public sealed class AppState
    {
        public HomeDTOs? Home { get; set; }
        public TeamDtos? Team { get; set; }
        public QuestionDtos? Question { get; set; }
        public SoloGameDtos? SoloGame { get; set; }

        public string Culture { get; set; } = "hu";
        public LocalStorageStates LocStoreStates { get; set; } = new LocalStorageStates();
    }
    public sealed class LocalStorageStates
    { 
        public bool? ChkBxNotShowNew { get; set; }
        public bool? ChkBxNotShowDel { get; set; }
        public DateTime LastBboardChk {get; set; } = DateTime.MinValue;

    }

}
