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

internal sealed record FactoryQuestionCategoryCount(
    int CategoryNo,
    int Count);

internal sealed record FactoryQuestionRow(
    int Id,
    int? PlayerId,
    int CategoryNo,
    string Question,
    string AnswerData,
    int Reported,
    bool IsTip)
{
    public override string ToString() =>
        $"{Id,-8} {CategoryNo,-5} {Shorten(Question, 53),-53} {Shorten(AnswerData, 20)}";

    private static string Shorten(string value, int maximumLength) =>
        value.Length <= maximumLength
            ? value
            : $"{value[..(maximumLength - 1)]}…";
}

internal sealed record AuditFileRow(string FilePath, string FileName)
{
    public override string ToString() => FileName;
}

internal sealed record AuditDetailsRow(
    string[]? ChangedFields,
    string? DocumentVersion);

internal sealed record AuditEntryRow(
    DateTimeOffset UtcTime,
    string EventName,
    string Outcome,
    string? ActorId,
    string? SubjectId,
    string? IpHash,
    string? RequestId,
    AuditDetailsRow? Details)
{
    public override string ToString() =>
        $"{UtcTime:HH:mm:ss}  {Outcome,-9}  {EventName,-36}  " +
        $"Actor: {ShortId(ActorId),-10}  Subject: {ShortId(SubjectId),-10}";

    private static string ShortId(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Length <= 8
                ? value
                : value[..8];
}

internal enum MigrationExecutionState
{
    NotRequired,
    Pending,
    Applied
}

internal sealed record MigrationTargetState(
    bool Required,
    string? Migration,
    string? File,
    MigrationExecutionState State,
    DateTimeOffset? AppliedAtUtc)
{
    public string DisplayText => State switch
    {
        MigrationExecutionState.NotRequired => "nem érintett",
        MigrationExecutionState.Applied when AppliedAtUtc.HasValue =>
            $"végrehajtva: {AppliedAtUtc.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss} ({Migration})",
        MigrationExecutionState.Applied => $"végrehajtva, időpont nincs ({Migration})",
        _ => $"feltöltve, nincs végrehajtva ({Migration})"
    };
}

internal sealed record MigrationSnapshot(
    Guid UploadId,
    DateTimeOffset UploadedAtUtc,
    MigrationTargetState Application,
    MigrationTargetState Game)
{
    public DateTimeOffset? FirstAppliedAtUtc =>
        new[] { Application.AppliedAtUtc, Game.AppliedAtUtc }
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Cast<DateTimeOffset?>()
            .Min();

    public string PackageState
    {
        get
        {
            var required = new[] { Application, Game }.Where(target => target.Required).ToArray();
            var applied = required.Count(target => target.State == MigrationExecutionState.Applied);
            return applied == 0
                ? "FELTÖLTVE, NINCS VÉGREHAJTVA"
                : applied == required.Length
                    ? "VÉGREHAJTVA"
                    : "RÉSZBEN VÉGREHAJTVA";
        }
    }
}

internal sealed record MigrationTrackingState(
    bool IsInitialized,
    long? ExecutionId,
    string? MigrationId,
    DateTimeOffset? AppliedAtUtc)
{
    public string DisplayText => !IsInitialized
        ? "nincs inicializálva"
        : MigrationId is null
            ? "inicializálva, még nincs migráció"
            : $"{MigrationId} — {AppliedAtUtc?.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
}

internal sealed record ReleaseRow(
    string Id,
    string DirectoryPath,
    bool IsActive,
    DateTimeOffset DeployedAtUtc,
    bool HasMigrationRisk)
{
    public override string ToString() =>
        $"{(IsActive ? "AKTÍV" : string.Empty),-7} {Id,-24} {DeployedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm}  " +
        (HasMigrationRisk ? "⚠ MIGRÁCIÓ ELŐTT" : string.Empty);
}

internal sealed record DeploymentSnapshot(
    string ServerState,
    MigrationSnapshot? Migration,
    IReadOnlyList<ReleaseRow> Releases);
