using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Models.ForAdminClient;
using KvizCommando.Server.Services;
using KvizCommando.Shared.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace KvizCommando.Server.Controllers.AdminAppControllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Api")]
    public class AdminAppController : ControllerBase
    {
        private readonly IAdminAppService _adminAppService;
        public AdminAppController(IAdminAppService adminAppService)
        {
            _adminAppService = adminAppService;
        }

        [HttpPost("import")]
        public async Task<string> ImportQuestions([FromBody] List<ModelQuestions> questions)
        {
            if (questions == null || questions.Count == 0)
                return "Üres lista érkezett.";
            List<FactoryQuestion> saveQuestionsList = new();


            Console.WriteLine("=== Fogadott kérdések ===");
            foreach (var q in questions)
            {
                Console.WriteLine($"ID: {q.Id}, Kategória: {q.Category}, Kérdés: {q.Question}");
                saveQuestionsList.Add(new FactoryQuestion
                {                   
                    CategoryNo = q.Category,
                    Question = q.Question,
                    AnswersJson = $"[\"{q.Answer1}\",\"{q.Answer2}\",\"{q.Answer3}\",\"{q.Answer4}\"]"
                });
            }
            var result = await _adminAppService.saveImportedQuestionsToDb(saveQuestionsList);

            Console.WriteLine("==========================");
            if (!result)
                return "Hiba a mentés során.";
            return "Success";
        }

        [HttpPost("pendingapprove")]
        public async Task<string> ApprovePendingQuestion([FromBody] ModelQuestions question)
        {
            if (question == null)
                return "Üres üzenet érkezett.";
            if (question.Id == 0)
                return "Hibás id.";
            PendingQuestion pq = new PendingQuestion
            {
                Id = question.Id,
                CategoryNo = question.Category,
                Question = question.Question,
                AnswersJson = $"[\"{question.Answer1}\",\"{question.Answer2}\",\"{question.Answer3}\",\"{question.Answer4}\"]",
                Status = (QuestionStatus)question.remoteStatus,
                Remark = question.remoteFeedBack
            };

            Console.WriteLine("=== Fogadott kérdések ===");
            Console.WriteLine($"ID: {pq.Id}, Kategória: {pq.CategoryNo}, Kérdés: {pq.Question}, status: {pq.Status}, remark: {pq.Remark}");

            
            var result = await _adminAppService.savePendingQuestionToDb(pq);

            Console.WriteLine("==========================");
            if (!result)
                return "Hiba a mentés során.";
            return "Success";
        }




        [HttpGet("factory")]
        public async Task<FactoryResponse> GetFactoryAsync([FromQuery] int category)
        {
            var dto = new List<ModelQuestions>() { };
            if (category>16) return new FactoryResponse { Message = $"Hibás category ID: {category}", Questions = dto };

            await Task.Delay(100);
         
            
            Console.WriteLine($"Lekérdezett kategória ID: {category}");
            Console.WriteLine("=== Küldésre szánt kérdések  ===");
            var questions = await _adminAppService.getFactoryQuestionsByCategoryAsync(category);

            if (questions == null || questions.Count == 0)
            {
                Console.WriteLine("Nincsenek kérdések ebben a kategóriában.");
                return new FactoryResponse
                {
                    Message = "Nincsenek kérdések ebben a kategóriában.",
                    Questions = dto
                };
            }
            foreach (var q in questions)
            {
                var answers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.AnswersJson);
                dto.Add(new ModelQuestions
                {
                    Id = q.Id,
                    Category = q.CategoryNo,
                    Question = q.Question,
                    Answer1 = answers != null && answers.Count > 0 ? answers[0] : "",
                    Answer2 = answers != null && answers.Count > 1 ? answers[1] : "",
                    Answer3 = answers != null && answers.Count > 2 ? answers[2] : "",
                    Answer4 = answers != null && answers.Count > 3 ? answers[3] : "",
                    Status = q.Reported > 0
                });
                Console.WriteLine($"ID: {q.Id}, Kategória: {q.CategoryNo}, Kérdés: {q.Question}");
            }
            Console.WriteLine("==========================");

            return new FactoryResponse
            {
                Message = "Success",
                Questions = dto
            };
        }

        [HttpGet("pending")]
        public async Task<FactoryResponse> GetPendingAsync([FromQuery] bool? status = null)
        {
            var dto = new List<ModelQuestions>() { };
            bool getAll = status ?? false;

            await Task.Delay(100);


            Console.WriteLine($"Lekérdezett : {getAll}");
            Console.WriteLine("=== Küldésre szánt kérdések  ===");
            var questions = await _adminAppService.getPendingQuestionsByCategoryAsync(getAll);

            if (questions == null || questions.Count == 0)
            {
                Console.WriteLine("Nincsenek kérdések ebben a kategóriában.");
                return new FactoryResponse
                {
                    Message = "Nincsenek visszaküldendő kérdések",
                    Questions = dto
                };
            }
            foreach (var q in questions)
            {
                var answers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.AnswersJson);
                dto.Add(new ModelQuestions
                {
                    Id = q.Id,
                    Category = q.CategoryNo,
                    Question = q.Question,
                    Answer1 = answers != null && answers.Count > 0 ? answers[0] : "",
                    Answer2 = answers != null && answers.Count > 1 ? answers[1] : "",
                    Answer3 = answers != null && answers.Count > 2 ? answers[2] : "",
                    Answer4 = answers != null && answers.Count > 3 ? answers[3] : "",
                    Status = q.Reported > 0,
                    remoteStatus = (int)q.Status,
                    remoteFeedBack = q.Remark ?? "Nincs"
                });
                Console.WriteLine($"ID: {q.Id}, Kategória: {q.CategoryNo}, Kérdés: {q.Question} Státusz:{q.Status.ToString()}  Remark: {q.Remark}");
            }
            Console.WriteLine("==========================");

            return new FactoryResponse
            {
                Message = "Success",
                Questions = dto
            };
        }






    }
}
