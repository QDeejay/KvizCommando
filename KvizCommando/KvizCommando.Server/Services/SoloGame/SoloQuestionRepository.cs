using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Services.SoloGame;
public sealed class SoloQuestionRepository : ISoloQuestionRepository
{
    private readonly GameDbContext _db;
    public SoloQuestionRepository(GameDbContext db) => _db = db;
    public async Task<IReadOnlyList<FactoryQuestion>> LoadByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var selected = ids.Distinct().ToArray();
        return await _db.FactoryQuestions.AsNoTracking()
            .Where(q => selected.Contains(q.Id)).ToListAsync(ct);
    }
}
