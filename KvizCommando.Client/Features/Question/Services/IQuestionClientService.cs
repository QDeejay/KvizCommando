using KvizCommando.Shared.Contracts.Question;

namespace KvizCommando.Client.Features.Question.Services;

public interface IQuestionClientService
{
    Task<bool> SaveFactorySlotsAsync(
        SaveFactoryRequest request,
        CancellationToken ct = default);

    Task<bool> ManageSlotAsync(
        ManageSlotRequest request,
        CancellationToken ct = default);

    Task<bool> SendNewQuestionAsync(
        NewQuestionRequest request,
        CancellationToken ct = default);
}
