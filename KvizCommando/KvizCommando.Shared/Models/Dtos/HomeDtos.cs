using KvizCommando.Shared.Models.User;


namespace KvizCommando.Shared.Models.Dtos
{
    public class HomeDTOs
    {
        public bool AccessDenied { get; set; } = false;
        public UserMainData UserMainData { get; set; } = new();

        public HomeScreen HomeScreen { get; set; } = new();

        public HomeExtendedInfo ExtendedInfo { get; set; } = new(); 

    }

    public class HomeExtendedInfo 
    { 
        public int NextXp { get; set; } = 0;

        public DateTime LastInfo { get; set; } = DateTime.MinValue;

    }

    public class HomeScreen
    {
        public ScreenButtonEntity VsGame { get; set; } = new();
        public ScreenButtonEntity SoloGame { get; set; } = new();
        public ScreenButtonEntity Team { get; set; } = new();
        public ScreenButtonEntity Shop { get; set; } = new();

        public ScreenButtonEntity Question { get; set; } = new();

        public ScreenButtonEntity Community { get; set; } = new();

        public ScreenButtonEntity Messages { get; set; } = new();

        public ScreenButtonEntity Statistic { get; set; } = new();
        public ScreenButtonEntity Events { get; set; } = new();
        public ScreenButtonEntity Ranking { get; set; } = new();
        public ScreenButtonEntity InfoBoard { get; set; } = new();

    }

    public class ScreenButtonEntity
    {
        public bool Enable { get; set; } = false;
        public int FooterData1 { get; set; } = 0;
        public int FooterData2 { get; set; } = 0;
        public int FooterData3 { get; set; } = 0;
    }

}
