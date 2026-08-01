using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static class VsMatchSnapshotBuilder
{
    internal static (
        string ConnectionId,
        VsMatchSnapshot Snapshot)[] BuildMessages(
            VsMatchSession match) =>
        [
            .. match.Players
                .Where(player => player.IsConnected)
                .Select(player => (
                    player.ConnectionId,
                    BuildSnapshot(match, player)))
        ];

    private static VsMatchSnapshot BuildSnapshot(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer) =>
        new()
        {
            MatchId = match.MatchId,
            ClassificationId =
                match.Classification.ClassificationId,
            Stake = match.Classification.Stake,
            Phase = match.Phase,
            DeadlineUtc = match.DeadlineUtc,
            PhaseDurationSeconds =
                ResolvePhaseDuration(match),
            InfoKey = ResolveInfoKey(match, currentPlayer),
            Players =
            [
                .. OrderPlayers(match).Select(player =>
                    new VsMatchPlayerDto
                    {
                        Position = player.Position,
                        DisplayName = player.IsBot
                            ? player.BotName
                            : player.DisplayName,
                        TeamName = player.TeamName,
                        TeamLevel = player.TeamLevel,
                        TeamPictureCode =
                            player.TeamPictureCode,
                        IsMe =
                            player.PlayerId ==
                            currentPlayer.PlayerId,
                        IsConnected = player.IsConnected,
                        IsBot = player.IsBot,
                        IsFinished = player.IsFinished,
                        TotalPoints = ResolveDisplayedPoints(
                            match,
                            player),
                        TotalTimeSeconds =
                            ResolveDisplayedTime(match, player),
                        ActiveCharacter =
                            BuildActiveCharacter(
                                match,
                                player)
                    })
            ],
            Preparation = BuildPreparation(
                match,
                currentPlayer),
            Game = BuildGame(match, currentPlayer),
            Reward = BuildReward(match, currentPlayer)
        };

    private static VsMatchRewardDto BuildReward(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        if (match.Phase != VsMatchPhase.GameCompleted ||
            match.Reward is null)
        {
            return new VsMatchRewardDto();
        }

        var myReward = match.Reward.Players.First(player =>
            player.PlayerId == currentPlayer.PlayerId);

        return new VsMatchRewardDto
        {
            PrizePool = match.Reward.PrizePool,
            Standings =
            [
                .. match.Reward.Players.Select(player =>
                    new VsRewardStandingDto
                    {
                        FinalPosition = player.FinalPosition,
                        PlayerPosition = player.OriginalPosition,
                        DisplayName = player.DisplayName,
                        TeamName = player.TeamName,
                        TeamLevel = player.TeamLevel,
                        IsMe = player.PlayerId == currentPlayer.PlayerId,
                        IsBot = player.IsBot,
                        IsWinner = player.IsWinner,
                        Points = player.FinalPoints,
                        TimeSeconds = player.FinalTimeSeconds
                    })
            ],
            MyReward = new VsMyRewardDto
            {
                FinalPosition = myReward.FinalPosition,
                IsBot = myReward.IsBot,
                IsTeamXpAvailable =
                    !myReward.IsBot && myReward.TeamLevel <= 21,
                CharacterAverageXp = myReward.CharacterAverageXp,
                ScoreXp = myReward.ScoreXp,
                TeamXp = myReward.TeamXp,
                StakeReturn = myReward.StakeReturn,
                BaseCreditReward = myReward.BaseCreditReward,
                TeamBonusCredit = myReward.TeamBonusCredit,
                TeamBonusPercent = myReward.TeamBonusPercent,
                CreditReward = myReward.CreditReward,
                ConsumedHelps = myReward.ConsumedHelps,
                Characters =
                [
                    .. myReward.Characters.Select(character =>
                        new VsCharacterRewardDto
                        {
                            SlotNumber = character.SlotNumber,
                            Name = character.Name,
                            PictureCode = character.PictureCode,
                            CharacterXp = character.CharacterXp,
                            EnergyLoss = character.EnergyLoss,
                            Pension = character.Pension
                        })
                ]
            }
        };
    }

    private static int ResolveDisplayedPoints(
        VsMatchSession match,
        VsMatchPlayerState player) =>
        match.Reward?.Players.FirstOrDefault(item =>
            item.PlayerId == player.PlayerId)?.FinalPoints ??
        player.TotalPoints;

    private static double ResolveDisplayedTime(
        VsMatchSession match,
        VsMatchPlayerState player) =>
        match.Reward?.Players.FirstOrDefault(item =>
            item.PlayerId == player.PlayerId)?.FinalTimeSeconds ??
        player.TotalTimeSeconds;

    private static VsGameDto BuildGame(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var game = match.Game;
        var question = game.CurrentQuestion;
        var showResult =
            match.Phase == VsMatchPhase.QuestionResult &&
            game.QuestionResult is not null;

        return new VsGameDto
        {
            CurrentRoundNumber = game.CurrentRoundNumber,
            NormalRoundCount =
                match.Classification.RequiredPartySize,
            QuestionNumber = game.QuestionNumber,
            QuestionKind = game.QuestionKind,
            QuestionerPosition =
                question?.QuestionerPosition ?? 0,
            Question = question?.Question ?? string.Empty,
            Answers = question?.Answers ?? [],
            CorrectAnswerIndex = showResult &&
                                 question?.Kind ==
                                 VsQuestionKind.Choice
                ? question!.CorrectOptionIndex
                : null,
            CorrectGuess = showResult &&
                           question?.Kind ==
                           VsQuestionKind.Guess
                ? question!.CorrectGuess
                : null,
            MyAnswerIndex =
                currentPlayer.CurrentAnswer?.AnswerIndex,
            MyGuess = currentPlayer.CurrentAnswer?.Guess,
            MyTimeModifierSeconds =
                ResolveCurrentTimeModifier(
                    match,
                    currentPlayer,
                    question),
            MyGuessRangeMinimum =
                currentPlayer.GuessRangeMinimum,
            MyGuessRangeMaximum =
                currentPlayer.GuessRangeMaximum,
            MyHiddenAnswerIndices =
                currentPlayer.HiddenAnswerIndices,
            MySuggestedAnswerIndex =
                currentPlayer.SuggestedAnswerIndex,
            MyHelpUsesRemaining =
                ResolveHelpUsesRemaining(
                    match,
                    currentPlayer),
            IsMyHelpUnlimited =
                ResolveCurrentHelpType(
                    match,
                    currentPlayer) ==
                VsHelpType.TimeFreeze,
            CanUseHelp = VsMatchGameRules.CanUseHelp(
                match,
                currentPlayer,
                DateTime.UtcNow),
            MyRoundPoints = currentPlayer.RoundPoints,
            MyRoundTimeSeconds =
                currentPlayer.RoundTimeSeconds,
            CanAnswer =
                IsAnswerPhase(match.Phase) &&
                currentPlayer.IsConnected &&
                currentPlayer.CurrentAnswer is null &&
                (!match.DeadlineUtc.HasValue ||
                 DateTime.UtcNow <= match.DeadlineUtc.Value),
            CanChooseCaptainQuestion =
                CanChooseCaptainQuestion(match, currentPlayer),
            QuestionPlayers = BuildQuestionPlayers(
                match,
                showResult),
            Progress = BuildProgress(match, currentPlayer),
            RoundResult =
            [
                .. game.RoundResult.Select(ToRoundResultDto)
            ],
            CaptainQuestions =
                BuildCaptainQuestions(match, currentPlayer),
            CaptainOrder = game.CaptainOrder,
            CaptainOrderIndex = game.CaptainOrderIndex
        };
    }

    private static double? ResolveCurrentTimeModifier(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer,
        VsMatchQuestionState? question)
    {
        if (match.Phase is not
                (VsMatchPhase.NormalRoundQuestion or
                 VsMatchPhase.QuestionResult) ||
            question is null ||
            question.Kind != VsQuestionKind.Choice ||
            question.QuestionerPosition == currentPlayer.Position ||
            match.Game.CurrentRoundNumber <= 0 ||
            match.Game.CurrentRoundNumber >
                match.Classification.RequiredPartySize)
        {
            return null;
        }

        if (match.Phase == VsMatchPhase.QuestionResult &&
            currentPlayer.ActiveQuestionHelp ==
                VsHelpType.TimeFreeze)
        {
            var result = match.Game.QuestionResult
                ?.Players.FirstOrDefault(item =>
                    item.Position == currentPlayer.Position);

            if (result is
                {
                    AnswerIndex: not null,
                    IsCorrect: false
                })
            {
                return match.Profile
                    .TimeFreezeWrongAnswerPenaltySeconds;
            }
        }

        var seconds = VsMatchScoring.CalculateTimeModifier(
            match,
            currentPlayer,
            question);

        return Math.Truncate(seconds * 10) / 10;
    }

    private static int ResolveHelpUsesRemaining(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var round = ResolveCurrentRound(
            match,
            currentPlayer);

        return round is
            {
                HelpType:
                    VsHelpType.FiftyFifty or
                    VsHelpType.AiSuggestion,
                HelpUsed: false
            }
                ? 1
                : 0;
    }

    private static VsHelpType ResolveCurrentHelpType(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer) =>
        ResolveCurrentRound(match, currentPlayer)
            ?.HelpType ?? VsHelpType.None;

    private static VsMatchRoundState? ResolveCurrentRound(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        if (match.Game.CurrentRoundNumber <= 0)
            return null;

        return currentPlayer.Rounds.FirstOrDefault(round =>
            round.RoundNumber ==
            match.Game.CurrentRoundNumber);
    }

    private static VsQuestionPlayerDto[] BuildQuestionPlayers(
        VsMatchSession match,
        bool showResult)
    {
        if (showResult)
        {
            return
            [
                .. match.Game.QuestionResult!.Players.Select(
                    result => new VsQuestionPlayerDto
                    {
                        Position = result.Position,
                        HasAnswered =
                            result.AnswerIndex.HasValue ||
                            result.Guess.HasValue,
                        AnswerIndex = result.AnswerIndex,
                        Guess = result.Guess,
                        IsCorrect = result.IsCorrect,
                        AnswerTimeSeconds =
                            result.AnswerTimeSeconds,
                        ModifiedTimeSeconds =
                            result.ModifiedTimeSeconds,
                        Points = result.Points,
                        HasSpeedBonus =
                            result.HasSpeedBonus
                    })
            ];
        }

        return
        [
            .. match.Players.Select(player =>
                new VsQuestionPlayerDto
                {
                    Position = player.Position,
                    HasAnswered =
                        player.CurrentAnswer
                            ?.QuestionNumber ==
                        match.Game.QuestionNumber
                })
        ];
    }

    private static VsRoundProgressDto[] BuildProgress(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var isCaptain =
            match.Game.CurrentRoundNumber >
            match.Classification.RequiredPartySize;
        var stepCount = isCaptain
            ? match.Players.Count
            : match.Players.Count + 1;
        var completedCount =
            currentPlayer.RoundProgress.Count;
        var currentStep = ResolveCurrentProgressStep(
            match,
            isCaptain);
        var questionOrder = ResolveQuestionOrder(match);
        var result = new List<VsRoundProgressDto>(stepCount);

        for (var index = 0; index < stepCount; index++)
        {
            var isGuess = !isCaptain && index == 0;
            var orderIndex = isCaptain ? index : index - 1;
            var playerPosition =
                isGuess ||
                orderIndex < 0 ||
                orderIndex >= questionOrder.Length
                    ? 0
                    : questionOrder[orderIndex];

            result.Add(new VsRoundProgressDto
            {
                StepNumber = index + 1,
                PlayerPosition = playerPosition,
                IsGuess = isGuess,
                IsCompleted = index < completedCount,
                IsCurrent = index == currentStep,
                Points = index < completedCount
                    ? currentPlayer.RoundProgress[index]
                    : 0
            });
        }

        return [.. result];
    }

    private static int ResolveCurrentProgressStep(
        VsMatchSession match,
        bool isCaptain)
    {
        if (match.Phase is
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainRoundResult or
            VsMatchPhase.GameCompleted)
        {
            return -1;
        }

        if (!isCaptain &&
            match.Game.QuestionKind ==
            VsQuestionKind.Guess)
        {
            return 0;
        }

        return isCaptain
            ? match.Game.CaptainOrderIndex
            : match.Game.CurrentQuestionerIndex + 1;
    }

    private static int[] ResolveQuestionOrder(
        VsMatchSession match) =>
        match.Game.CurrentRoundNumber >
        match.Classification.RequiredPartySize
            ? match.Game.CaptainOrder
            : match.Game.QuestionerOrder;

    private static VsCaptainQuestionDto[] BuildCaptainQuestions(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        if (!CanChooseCaptainQuestion(match, currentPlayer))
            return [];

        return
        [
            .. VsMatchGameRules
                .GetCaptainChoices(match, currentPlayer)
                .Select(item => new VsCaptainQuestionDto
                {
                    LoadoutPosition =
                        item.LoadoutPosition,
                    CategoryId = item.CategoryId,
                    Question = item.Question
                })
        ];
    }

    private static bool CanChooseCaptainQuestion(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer) =>
        match.Phase ==
            VsMatchPhase.CaptainQuestionSelection &&
        currentPlayer.IsConnected &&
        match.Game.CaptainOrder.Length >
            match.Game.CaptainOrderIndex &&
        currentPlayer.Position ==
            match.Game.CaptainOrder[
                match.Game.CaptainOrderIndex];

    private static VsRoundResultDto ToRoundResultDto(
        VsMatchRoundResultState result) =>
        new()
        {
            Position = result.Position,
            TotalBefore = result.TotalBefore,
            RoundPoints = result.RoundPoints,
            TotalAfter = result.TotalAfter,
            RoundTimeSeconds = result.RoundTimeSeconds,
            HasWinnerBonus = result.HasWinnerBonus,
            HasFastestBonus = result.HasFastestBonus,
            CharacterSlotNumber =
                result.CharacterSlotNumber,
            CharacterXp = result.CharacterXp,
            EnergyLoss = result.EnergyLoss
        };

    private static VsPreparationDto BuildPreparation(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var assignedCharacterSlots = currentPlayer.Rounds
            .Where(round =>
                round.CharacterSlotNumber.HasValue)
            .Select(round =>
                round.CharacterSlotNumber!.Value)
            .ToHashSet();

        var assignedLoadoutPositions = currentPlayer.Rounds
            .Where(round => round.LoadoutPosition.HasValue)
            .Select(round => round.LoadoutPosition!.Value)
            .ToHashSet();

        return new VsPreparationDto
        {
            TeamSize = match.Classification.RequiredPartySize,
            IsFinished = currentPlayer.IsFinished,
            CanReset = VsMatchPreparationRules.CanReset(
                match.Phase,
                currentPlayer),
            CanFinish = !currentPlayer.IsFinished &&
                        VsMatchPreparationRules.CanFinish(
                            match.Phase,
                            currentPlayer),
            Rounds =
            [
                .. currentPlayer.Rounds.Select(round =>
                    BuildRound(currentPlayer, round))
            ],
            CharacterInventory =
            [
                .. currentPlayer.Characters
                    .Where(character =>
                        !assignedCharacterSlots.Contains(
                            character.SlotNumber))
                    .Select(ToCharacterDto)
            ],
            LoadoutInventory =
            [
                .. currentPlayer.Loadout
                    .Where(item =>
                        !assignedLoadoutPositions.Contains(
                            item.LoadoutPosition))
                    .OrderBy(item => item.LoadoutPosition)
                    .Select(ToLoadoutDto)
            ],
            HelpInventory =
            [
                .. Enum.GetValues<VsHelpType>()
                    .Where(help => help != VsHelpType.None)
                    .Select(help => new VsHelpCardDto
                    {
                        HelpType = help,
                        Count =
                            currentPlayer.HelpCounts[(int)help - 1] > 0 &&
                            currentPlayer.Rounds.All(round =>
                                round.HelpType != help)
                                ? 1
                                : 0
                    })
            ],
            CategoryModifiers = BuildCategoryModifiers(
                match,
                currentPlayer)
        };
    }

    private static VsPreparationRoundDto BuildRound(
        VsMatchPlayerState player,
        VsMatchRoundState round)
    {
        var character = round.CharacterSlotNumber.HasValue
            ? player.Characters.FirstOrDefault(item =>
                item.SlotNumber ==
                round.CharacterSlotNumber.Value)
            : null;

        var loadout = round.LoadoutPosition.HasValue
            ? player.Loadout.FirstOrDefault(item =>
                item.LoadoutPosition ==
                round.LoadoutPosition.Value)
            : null;

        return new VsPreparationRoundDto
        {
            RoundNumber = round.RoundNumber,
            IsCaptainRound = round.IsCaptainRound,
            Character = character is null
                ? null
                : ToCharacterDto(character),
            Loadout = loadout is null
                ? null
                : ToLoadoutDto(loadout),
            HelpType = round.HelpType
        };
    }

    private static VsCategoryModifierDto[] BuildCategoryModifiers(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var result = new List<VsCategoryModifierDto>();

        foreach (var round in currentPlayer.Rounds.Where(round =>
                     !round.IsCaptainRound))
        {
            for (var categoryId =
                     VsLoadoutCategoryIds.MinimumFactoryCategory;
                 categoryId <=
                     VsLoadoutCategoryIds.MaximumFactoryCategory;
                 categoryId++)
            {
                var seconds = 0d;

                foreach (var otherPlayer in match.Players.Where(player =>
                             player.PlayerId != currentPlayer.PlayerId))
                {
                    var otherRound =
                        otherPlayer.Rounds.First(item =>
                            item.RoundNumber == round.RoundNumber);

                    if (!otherRound.CharacterSlotNumber.HasValue)
                        continue;

                    var character =
                        otherPlayer.Characters.First(item =>
                            item.SlotNumber ==
                            otherRound.CharacterSlotNumber.Value);

                    if (character.CategoryModifiers.TryGetValue(
                            categoryId,
                            out var modifier))
                    {
                        seconds += modifier;
                    }
                }

                result.Add(new VsCategoryModifierDto
                {
                    RoundNumber = round.RoundNumber,
                    CategoryId = categoryId,
                    Seconds = Math.Truncate(seconds * 10) / 10
                });
            }
        }

        return [.. result];
    }

    private static VsCharacterCardDto ToCharacterDto(
        VsMatchCharacterState character) =>
        new()
        {
            SlotNumber = character.SlotNumber,
            Name = character.Name,
            PictureCode = character.PictureCode,
            Level = character.Level,
            OrientationId = character.OrientationId
        };

    private static VsCharacterCardDto? BuildActiveCharacter(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        if (match.Game.CurrentRoundNumber <= 0 ||
            match.Game.CurrentRoundNumber >
                match.Classification.RequiredPartySize ||
            match.Phase is
                VsMatchPhase.MatchLocked or
                VsMatchPhase.PreparationOrder or
                VsMatchPhase.PreparationCategories or
                VsMatchPhase.PreparationHelps or
                VsMatchPhase.GameStarting)
        {
            return null;
        }

        var round = player.Rounds.First(item =>
            item.RoundNumber ==
            match.Game.CurrentRoundNumber);

        if (!round.CharacterSlotNumber.HasValue)
            return null;

        var character = player.Characters.First(item =>
            item.SlotNumber ==
            round.CharacterSlotNumber.Value);

        return ToCharacterDto(character);
    }

    private static VsLoadoutCardDto ToLoadoutDto(
        VsMatchLoadoutItemState item) =>
        new()
        {
            LoadoutPosition = item.LoadoutPosition,
            CategoryId = item.CategoryId,
            IsOwnQuestion = item.IsOwnQuestion,
            IsAllCategories = item.IsAllCategories,
            IsSelectable = !item.IsOwnQuestion
        };

    private static string ResolveInfoKey(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        if (player.IsFinished &&
            match.Phase is
                VsMatchPhase.PreparationOrder or
                VsMatchPhase.PreparationCategories or
                VsMatchPhase.PreparationHelps)
        {
            return "vsgame.Match.Info.WaitingForPlayers";
        }

        return match.Phase switch
        {
            VsMatchPhase.MatchLocked =>
                "vsgame.Match.Info.Locked",
            VsMatchPhase.PreparationStarting =>
                "vsgame.Match.Info.PreparationStarting",
            VsMatchPhase.PreparationOrder =>
                "vsgame.Match.Info.Order",
            VsMatchPhase.PreparationCategories =>
                "vsgame.Match.Info.Categories",
            VsMatchPhase.PreparationHelps =>
                "vsgame.Match.Info.Helps",
            VsMatchPhase.PreparationCompleted =>
                "vsgame.Match.Info.PreparationCompleted",
            VsMatchPhase.GameStarting =>
                "vsgame.Match.Info.GameStarting",
            VsMatchPhase.NormalRoundGuess =>
                "vsgame.Match.Info.Guess",
            VsMatchPhase.NormalRoundQuestion =>
                "vsgame.Match.Info.Question",
            VsMatchPhase.QuestionResult =>
                "vsgame.Match.Info.QuestionResult",
            VsMatchPhase.NormalRoundResult =>
                "vsgame.Match.Info.RoundResult",
            VsMatchPhase.CaptainQuestionSelection =>
                "vsgame.Match.Info.CaptainSelection",
            VsMatchPhase.CaptainQuestion =>
                "vsgame.Match.Info.CaptainQuestion",
            VsMatchPhase.CaptainRoundResult =>
                "vsgame.Match.Info.CaptainResult",
            VsMatchPhase.GameCompleted =>
                "vsgame.Match.Info.GameCompleted",
            _ => "vsgame.Match.Info.Aborted"
        };
    }

    private static IEnumerable<VsMatchPlayerState> OrderPlayers(
        VsMatchSession match)
    {
        if (match.Reward is not null)
        {
            return match.Reward.Players.Select(reward =>
                match.Players.First(player =>
                    player.PlayerId == reward.PlayerId));
        }

        return IsGamePhase(match.Phase)
            ? VsMatchScoring.OrderByStanding(match.Players)
            : match.Players.OrderBy(player => player.Position);
    }

    private static bool IsGamePhase(VsMatchPhase phase) =>
        phase is
            VsMatchPhase.GameStarting or
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.QuestionResult or
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainQuestionSelection or
            VsMatchPhase.CaptainQuestion or
            VsMatchPhase.CaptainRoundResult or
            VsMatchPhase.GameCompleted;

    private static int ResolvePhaseDuration(
        VsMatchSession match) =>
        match.Phase switch
        {
            VsMatchPhase.PreparationOrder or
            VsMatchPhase.PreparationCategories or
            VsMatchPhase.PreparationHelps =>
                match.Profile.PreparationSeconds,
            VsMatchPhase.PreparationStarting or
            VsMatchPhase.GameStarting =>
                match.Profile.PhasePauseSeconds,
            VsMatchPhase.NormalRoundGuess =>
                match.Profile.GuessSeconds,
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestion =>
                match.Profile.QuestionSeconds,
            VsMatchPhase.QuestionResult =>
                match.Profile.QuestionPauseSeconds,
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainRoundResult =>
                match.Profile.RoundResultSeconds,
            VsMatchPhase.CaptainQuestionSelection =>
                match.Profile.CaptainSelectionSeconds,
            _ => 0
        };

    private static bool IsAnswerPhase(
        VsMatchPhase phase) =>
        phase is
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestion;
}

/**
 * MÓDOSÍTÁS: a snapshot a későbbi reklamációhoz megtartja a publikus
 * MatchId hivatkozást, technikai fázisverziót nem küld. A
 * loadoutkiosztást a stabil LoadoutPosition alapján építi.
 *
 * MÓDOSÍTÁS: személyre szabott játéksnapshotot, aktuális karaktert,
 * élő rangsort, kérdésállapotot, progresszt és köreredményt épít. A
 * helyes válasz és az ellenfelek választása csak QuestionResult
 * fázisban jelenik meg.
 * MÓDOSÍTÁS: normál kérdésnél a saját időmódosítót csak a nem
 * kérdező játékos személyre szabott snapshotja tartalmazza.
 * MÓDOSÍTÁS: a preparáció előtti kezdési fázishoz lokalizációs kulcsot
 * és a meglévő fázisszünet hosszát küldi.
 * MÓDOSÍTÁS: a kapitány kérdésválasztásánál a külön
 * CaptainSelectionSeconds időtartamot küldi a kliensnek.
 * MÓDOSÍTÁS: a saját snapshot tartalmazza a tippsávot, az elrejtett
 * és javasolt választ, a segítség használhatóságát, valamint az
 * időtlenítőből eredő aktuális módosítót.
 * MÓDOSÍTÁS: hibás, beküldött időtlenítős válasz eredményfázisában
 * a saját kártya a -99 helyett a profil szerinti +20 büntetést kapja.
 * MÓDOSÍTÁS: GameCompleted fázisban a reward végső sorrendjét és
 * pont-/időadatait használja, a címzettnek csak a saját részletes
 * jutalmát küldi; a bot nyilvános neve és jelzője is innen kerül ki.
 * A team bonus százalékát, valamint a karakterátlag-, pontszám- és
 * összes csapat-XP-t is a személyre szabott reward tartalmazza.
 *
 * A szerveroldali meccsállapotból játékosonként tiszta SignalR-
 * snapshotokat épít. Nem módosít állapotot és nem küld üzenetet.
 */
