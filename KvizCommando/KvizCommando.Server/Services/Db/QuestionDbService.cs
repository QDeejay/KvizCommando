using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.Json;

namespace KvizCommando.Server.Services.Db
{
    public sealed class QuestionDbService : IQuestionDbService
    {
        private readonly GameDbContext _GameDb;
        public QuestionDbService(GameDbContext gamedb)
        {
            _GameDb = gamedb;
        }

        public async Task<CachedQuestion?> LoadQuestionsFromDbAsync(
           int playerId,
           CancellationToken ct)
        {
            var cq = new CachedQuestion();
            var questionsUsr = await GetUserSlotsAsync(playerId, ct);
            var questionsPend = await GetPendingSlotsAsync(playerId, ct);

            for (var i = 0; i < questionsUsr.Length; i++)
            {
                if (questionsUsr[i].CategoryNo == 0) questionsUsr[i] = new UserQuestion { Id = questionsUsr[i].Id, PlayerId = playerId }; ;
                cq.uSlots[i] = questionsUsr[i];
            }
            for (var i = 0; i < questionsPend.Length; i++)
            {
                if (questionsPend[i].CategoryNo == 0) questionsPend[i] = new PendingQuestion { Id = questionsPend[i].Id, PlayerId = playerId, Status = 0 }; ;
                cq.pSlots[i] = questionsPend[i];
            }
            cq.DirtyMask = 0;
            return cq;
        }

        
        public async Task<QuestionStats> SaveQuestionsToDbAsync(
            CachedQuestion cache,
            CancellationToken ct = default)
        {
            var qStats = new QuestionStats();
            try
            {
                var cp = cache;

                for (int i = 0; i < cp.uSlots.Length; i++)
                {
                    if ((cp.DirtyMask & 1u << i) != 0)
                    {
                        var usrQ = cp.uSlots[i];
                        if (usrQ.Id > 0) //Console.WriteLine($"Update: UserSlot{i} Question:{usrQ.Question}");
                       _GameDb.Update(usrQ);

                        else //Console.WriteLine($"Add: UserSlot{i} Question:{usrQ.Question}");
                           _GameDb.Add(usrQ);
                        qStats.totalQuestions++;
                        qStats.userQuestions++;
                    }
                }
                for (int i = 0; i < cp.pSlots.Length; i++)
                {
                    if ((cp.DirtyMask & 1u << i + 16) != 0)
                    {
                        var pendQ = cp.pSlots[i];
                        if (pendQ.Id > 0) //Console.WriteLine($"Update: PendingSlot{i} Question:{pendQ.Question}");
                        _GameDb.Update(pendQ);
                        else //Console.WriteLine($"Add: PendingSlot{i} Question:{pendQ.Question}");
                         _GameDb.Add(pendQ);
                        qStats.totalQuestions++;
                        qStats.pendingQuestions++;
                    }
                   
                }
                Console.WriteLine($"List before the update: {cache.fSlots.Count}");
                for (int i = cache.fSlots.Count-1; i >= 0; i--)
                { 
                    var factQ = cache.fSlots[i];
                    Console.WriteLine($"category:{factQ.CategoryNo} Question:{factQ.Question} Answers:{factQ.AnswersJson}");
                    if (factQ!=null && factQ.CategoryNo !=0 && !string.IsNullOrEmpty(factQ.Question) && !string.IsNullOrEmpty(factQ.AnswersJson))
                    {
                        Console.WriteLine($"Add: FactorySlot{i} Question:{factQ.Question}");
                        _GameDb.Add(factQ);
                            qStats.totalQuestions++;
                            qStats.transferedQuestions++;
                    }
                    
                }
                await _GameDb.SaveChangesAsync(ct);
                return qStats;
                
            }

            catch
            {
                return new QuestionStats();
                throw;
            }
            finally
            {
              
            }
        }

        private async Task<UserQuestion[]> GetUserSlotsAsync(int playerId, CancellationToken ct)
        {
            var result = await _GameDb.UserQuestions
              .Where(q => q.PlayerId == playerId)
              .OrderBy(q => q.Id)
               .Take(10)
               .ToListAsync(ct);
            // Ha kevesebb mint 10, pótoljuk új üres elemekkel
            while (result.Count < 10)
            {
                result.Add(new UserQuestion());
            }
            return result.ToArray();
        }
        private async Task<PendingQuestion[]> GetPendingSlotsAsync(int playerId, CancellationToken ct)
        {
            var result = await _GameDb.PendingQuestions
              .Where(q => q.PlayerId == playerId)
              .OrderBy(q => q.Id)
                    .Take(5)
                     .ToListAsync(ct);
            // Ha kevesebb mint 10, pótoljuk új üres elemekkel
            while (result.Count < 5)
            {
                result.Add(new PendingQuestion());
            }
            return result.ToArray();
        }

    }

}
