using KvizCommando.Server.Domain.Entities.Questions;
using System.Threading.Tasks;

namespace KvizCommando.Server.Services
{
    public interface IAdminAppService
    {
        Task<bool> saveImportedQuestionsToDb(List<FactoryQuestion> dto);
        Task<bool> savePendingQuestionToDb(PendingQuestion dto);
        Task<List<FactoryQuestion>> getFactoryQuestionsByCategoryAsync(int category);

        Task<List<PendingQuestion>> getPendingQuestionsByCategoryAsync(bool status);
    }
}

