using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using System.Threading.Tasks;

namespace KvizCommando.Server.Services.DtoMapping
{
    public interface IQuestionService
    {
     
        Task<bool> SaveFactorySlotsAsync(int playerId, SaveFactoryRequest dto, CancellationToken ct);
        Task<bool> ManageSlotsAsync(int playerId, ManageSlotRequest dto, CancellationToken ct);
        Task<bool> SendNewQuestionAsync(int playerId, NewQuestionRequest dto, CancellationToken ct);

    }
}
