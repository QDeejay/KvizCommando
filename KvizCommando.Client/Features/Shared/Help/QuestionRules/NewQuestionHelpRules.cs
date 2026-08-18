using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Help.QuestionRules;

public static class NewQuestionHelpRules
{
    public static IReadOnlyDictionary<string, string> Tokens { get; } =
        new Dictionary<string, string>
        {
            ["NEW_QUESTION_MIN_LENGTH"] =
                NewQuestionRules.QUESTION_MIN_LENGTH.ToString(),
            ["NEW_QUESTION_MAX_LENGTH"] =
                NewQuestionRules.QUESTION_MAX_LENGTH.ToString(),
            ["NEW_QUESTION_ANSWER_MAX_LENGTH"] =
                NewQuestionRules.ANSWER_MAX_LENGTH.ToString()
        };
}
