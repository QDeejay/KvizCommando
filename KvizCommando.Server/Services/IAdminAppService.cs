using KvizCommando.Server.Domain.Entities.Questions;
using System.Threading.Tasks;

namespace KvizCommando.Server.Services
{
    public interface IAdminAppService
    {
        /// <summary>
        /// Elmenti az importált kérdéseket az adatbázisba.
        /// </summary>
        Task<bool> saveImportedQuestionsToDb(List<FactoryQuestion> dto);
        /// <summary>
        /// Elmenti a függőben lévő kérdést az adatbázisba.
        /// </summary>
        Task<bool> savePendingQuestionToDb(PendingQuestion dto);
        /// <summary>
        /// Lekéri a megadott kategória gyári kérdéseit.
        /// </summary>
        Task<List<FactoryQuestion>> getFactoryQuestionsByCategoryAsync(int category);

        /// <summary>
        /// Lekéri a megadott kategória függőben lévő kérdéseit.
        /// </summary>
        Task<List<PendingQuestion>> getPendingQuestionsByCategoryAsync(bool status);
    }
}
