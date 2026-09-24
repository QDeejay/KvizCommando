namespace KvizCommando.Shared.Models.Dtos;

public sealed class RankingDtos
{
    public bool AccessDenied { get; set; } = false;
    public DateTime LastFlushUtc { get; set; }
    public int FlushIntervalSeconds { get; set; }
    public RankingListDto SoloOverall { get; set; } = new();
    public RankingListDto SoloCategory { get; set; } = new();
    public RankingListDto SoloOrientation { get; set; } = new();
    public RankingListDto VsAllTime { get; set; } = new();
}

public sealed class RankingListDto
{
    public int? CurrentPosition { get; set; }
    public RankingRowDto[] Rows { get; set; } = [];
}

public sealed class RankingRowDto
{
    public int Position { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int RankEnum { get; set; }
    public double Score { get; set; }
    public double? TimeDifferenceSeconds { get; set; }
    public bool IsCurrentPlayer { get; set; }
    public bool HasGapBefore { get; set; }
}
