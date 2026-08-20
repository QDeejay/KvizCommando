using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Solo.Services;
using KvizCommando.Client.Features.Solo.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums.VsGame;
using KvizCommando.Shared.Models.Rules;
using KvizCommando.Shared.Models.User;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace KvizCommando.Client.Features.Solo.Components;

partial class SoloGameManager
{
    private SoloPanelViewData BuildPanelData()
    {
        if (_phase == SoloGamePhase.Reward)
            return BuildRewardPanel();

        if (_phase is SoloGamePhase.Status or SoloGamePhase.Failed)
        {
            return new SoloPanelViewData
            {
                Mode = _statusKey ==
                    "solo.Label.GameProcess.Connecting"
                        ? SoloPanelMode.Connection
                        : SoloPanelMode.Status,
                DisplayLines =
                [
                    new SoloDisplayLine { ResourceKey = _statusKey }
                ],
                Progress = _progress
            };
        }

        var question = _game?.Questions.ElementAtOrDefault(_questionIndex);

        return new SoloPanelViewData
        {
            Mode = _phase == SoloGamePhase.Evaluation
                ? SoloPanelMode.Evaluation
                : SoloPanelMode.Question,
            DisplayLines = question is null
                ? []
                : [new SoloDisplayLine { Text = question.Question }],
            Answers = question?.Answers ?? [],
            Progress = _progress,
            SelectedAnswerIndex =
                _answers.ElementAtOrDefault(_questionIndex)?.SelectedOptionIndex ?? -1,
            CurrentAnswerResult =
                _phase == SoloGamePhase.Evaluation &&
                _questionIndex < _evaluatedCount
                    ? _result?.AnswerResults[_questionIndex]
                    : null,
            AnswerEnabled = _answerEnabled
        };
    }

    private SoloPanelViewData BuildRewardPanel()
    {
        var answered = _answers.Count(answer => answer.SelectedOptionIndex >= 0);

        return new SoloPanelViewData
        {
            Mode = SoloPanelMode.Reward,
            Reward = new SoloRewardViewData
            {
                Answered = answered,
                TotalQuestions = _answers.Length,
                Correct = _result?.CorrectAnswers ?? 0,
                Time = FormatTime(_result?.TotalAnswerTimeMs ?? 0),
                TotalPoints = _result?.TotalPoints.Sum() ?? 0,
                IsNewHighScore = _result?.IsNewHighScore == true,
                TeamXp = _result?.Rewards.TeamXp ?? 0,
                TeamDevPoints = _result?.Rewards.TeamDevPoints ?? 0,
                MemberXp = _result?.Rewards.MemberXp ?? 0,
                IsExperienceGame = IsExperienceGame,
                IsMemberXpCapped =
                    _result?.Rewards.IsMemberXpCapped == true,
                MemberDevPoints = _result?.Rewards.MemberDevPoints ?? 0,
                NewTeamLevel = _result?.Rewards.NewTeamLevel ?? 0,
                HealingPointAwarded =
                    _result?.Rewards.HealingPointAwarded == true
            },
            Progress = _progress
        };
    }

    private SoloPlayerViewData BuildPlayerProfile()
    {
        if (Mode == SoloGameMode.Category)
        {
            return new SoloPlayerViewData
            {
                Name = UserData.UserName,
                RankName = RankNameLocalizer.GetName(
                    UserData.RankEnum,
                    Culture),
                Level = RankNameTable.Data[UserData.RankEnum].PublicLevel
                    ?? "N/A",
                OrientationName = OrientationLocalizer.GetOrientation(
                    9,
                    Culture),
                ImageSrc = AvatarImageSrc(UserData.CaptainAvatar)
            };
        }

        var member = Members[SelectionId];

        return new SoloPlayerViewData
        {
            Name = member.Name,
            RankName = RankNameLocalizer.GetName(member.Level, Culture),
            Level = RankNameTable.Data[member.Level].PublicLevel ?? "N/A",
            OrientationName = OrientationLocalizer.GetOrientation(
                SelectionId,
                Culture),
            PictureCode = member.PictureCode,
            SoloBestScore = member.SoloBestScore
        };
    }

    private static string AvatarImageSrc(string? avatar) =>
        ProfileRules.TryGetAvatarNumber(avatar, out var avatarNumber)
            ? $"images/avatars/avatar-{avatarNumber:D2}.webp"
            : $"images/avatars/avatar-{ProfileRules.DEFAULT_AVATAR_NO:D2}.webp";

    private static int CalculateAnswerPoints(
        int maximumPoints,
        int elapsedMs) =>
        SoloGameRules.GetAnswerPoints(
            maximumPoints,
            elapsedMs);

    private static string EvaluationEffect(
        SoloQuestionState questionState) =>
        questionState switch
        {
            SoloQuestionState.Correct =>
                AudioService.SFX_HIT,
            SoloQuestionState.Unanswered =>
                AudioService.SFX_EMPTY,
            _ => AudioService.SFX_MISS
        };

    private static string FormatTime(int milliseconds) =>
        TimeSpan.FromMilliseconds(milliseconds).ToString(@"mm\:ss");
}
