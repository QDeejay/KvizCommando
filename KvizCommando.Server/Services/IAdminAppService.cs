using KvizCommando.Server.Domain.Entities.Questions;
using System.Threading.Tasks;

namespace KvizCommando.Server.Services
{
    public interface IAdminAppService
    {
        /// <summary>
        /// Elmenti az importált kérdéseket az adatbázisba.
        /// </summary>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        Task<bool> saveImportedQuestionsToDb(List<FactoryQuestion> dto);
        /// <summary>
        /// Elmenti a függőben lévő kérdést az adatbázisba.
        /// </summary>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        Task<bool> savePendingQuestionToDb(PendingQuestion dto);
        /// <summary>
        /// Lekéri a megadott kategória gyári kérdéseit.
        /// </summary>
        /// <param name="category">A lekérdezett kérdéskategória azonosítója.</param>
        Task<List<FactoryQuestion>> getFactoryQuestionsByCategoryAsync(int category);

        /// <summary>
        /// Lekéri a megadott kategória függőben lévő kérdéseit.
        /// </summary>
        /// <param name="status">A függőben lévő kérdések szűréséhez használt állapot.</param>
        Task<List<PendingQuestion>> getPendingQuestionsByCategoryAsync(bool status);
    }
}
