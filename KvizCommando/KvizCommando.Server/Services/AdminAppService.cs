using CsvHelper.TypeConversion;
using KvizCommando.Client.Pages.Question;
using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Shared.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace KvizCommando.Server.Services
{
    public class AdminAppService : IAdminAppService
    {
        private readonly GameDbContext _GameDb;
        public AdminAppService(GameDbContext gamedb)
        {
            _GameDb = gamedb;
        }

        public async Task<bool> saveImportedQuestionsToDb(List<FactoryQuestion> dto)
        {
            try 
            {
                foreach (var q in dto)
                {
                    bool exists = await _GameDb.FactoryQuestions
                        .AnyAsync(x => x.Question == q.Question && x.CategoryNo == q.CategoryNo);

                    if (!exists)
                        await _GameDb.FactoryQuestions.AddAsync(q);
                }
                await _GameDb.SaveChangesAsync();
                return true;
            } 
            catch 
            {
                return false;
            }
        }

        public async Task<bool> savePendingQuestionToDb(PendingQuestion dto)
        {

            try
            {
                int playerId = _GameDb.PendingQuestions
                                .Where(q => q.Id == dto.Id)
                                .Select(q => q.PlayerId)
                                .FirstOrDefault();
                dto.PlayerId = playerId;
                _GameDb.PendingQuestions.Update(dto);
                await _GameDb.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task<List<FactoryQuestion>> getFactoryQuestionsByCategoryAsync(int category)
        {
            var questions = await _GameDb.FactoryQuestions
                .Where(q => category == 0 || q.CategoryNo == category)
                .OrderByDescending(q => q.Id)
                .ToListAsync();

            return questions;
        }
        public async Task<List<PendingQuestion>> getPendingQuestionsByCategoryAsync(bool status)
        {
            var questions = await _GameDb.PendingQuestions
                .Where(q => status ?  q.Status != QuestionStatus.None : q.Status == QuestionStatus.Pending)
                .ToListAsync();

            return questions;
        }



    }
}
