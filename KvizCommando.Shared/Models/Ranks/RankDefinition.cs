namespace KvizCommando.Shared.Models.Ranks;

public sealed record RankDefinition(
    int EnumLevel,
    string PublicLevel,
    string NameHu,
    string ShortHu,
    string NameEn,
    string ShortEn);
