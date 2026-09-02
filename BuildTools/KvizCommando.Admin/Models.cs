namespace KvizCommando.Admin;

internal sealed record UserRow(
    string Id,
    string Email,
    string? DisplayName,
    bool EmailConfirmed,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    int? PlayerId,
    int? Rank,
    int? XP,
    int? Credit,
    int? Voucher)
{
    public override string ToString() =>
        $"{Email,-34}  {DisplayName ?? "-",-20}  {(EmailConfirmed ? "CONF" : "NO"),-4}  " +
        $"R:{Rank?.ToString() ?? "-",2}  XP:{XP?.ToString() ?? "-",6}  C:{Credit?.ToString() ?? "-",6}";
}

internal sealed record PendingQuestionRow(
    int Id,
    int PlayerId,
    int CategoryNo,
    string Question,
    string AnswersJson,
    string Status,
    string? Remark,
    DateTime SubmittedAt)
{
    public override string ToString() =>
        $"#{Id,-6} P:{PlayerId,-5} K:{CategoryNo,-2} {Status,-9} {Question}";
}

internal sealed record UserQuestionRow(
    int Id,
    int PlayerId,
    int CategoryNo,
    string Question,
    string AnswersJson,
    int Ask,
    int OkAnswer)
{
    public override string ToString() =>
        $"#{Id,-6} P:{PlayerId,-5} K:{CategoryNo,-2} {Question}";
}
