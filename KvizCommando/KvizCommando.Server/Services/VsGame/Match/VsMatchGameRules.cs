using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static class VsMatchGameRules
{
    internal static void BeginFirstNormalRound(
        VsMatchSession match)
    {
        match.Game.CurrentRoundNumber = 1;
        BeginNormalRound(match);
    }

    internal static void BeginNextNormalRound(
        VsMatchSession match)
    {
        match.Game.CurrentRoundNumber++;
        BeginNormalRound(match);
    }

    internal static void BeginNormalQuestion(
        VsMatchSession match)
    {
        ClearAnswers(match);

        var questionerPosition =
            match.Game.QuestionerOrder[
                match.Game.CurrentQuestionerIndex];
        var questioner = FindByPosition(
            match,
            questionerPosition);
        var round = questioner.Rounds.First(item =>
            item.RoundNumber ==
            match.Game.CurrentRoundNumber);
        var loadout = questioner.Loadout.First(item =>
            item.LoadoutPosition ==
            round.LoadoutPosition);

        match.Game.QuestionNumber++;
        match.Game.QuestionKind = VsQuestionKind.Choice;
        match.Game.CurrentQuestion =
            CreateChoiceQuestion(
                loadout,
                questionerPosition);
        match.Game.QuestionResult = null;
    }

    internal static void BeginCaptainRound(
        VsMatchSession match)
    {
        match.Game.CurrentRoundNumber =
            match.Classification.RequiredPartySize + 1;
        ResetRoundState(match);
        match.Game.CaptainOrder =
        [
            .. VsMatchScoring
                .OrderByStanding(match.Players)
                .Reverse()
                .Select(player => player.Position)
        ];
        match.Game.CaptainOrderIndex = 0;
        match.Game.QuestionKind = VsQuestionKind.None;
        match.Game.CurrentQuestion = null;
        match.Game.QuestionResult = null;
        match.Game.RoundResult = [];
    }

    internal static bool SelectCaptainQuestion(
        VsMatchSession match,
        VsMatchPlayerState? player,
        VsCaptainQuestionRequest request)
    {
        if (match.Phase !=
                VsMatchPhase.CaptainQuestionSelection ||
            player is null ||
            !player.IsConnected ||
            player.Position !=
                match.Game.CaptainOrder[
                    match.Game.CaptainOrderIndex])
        {
            return false;
        }

        var loadout = GetCaptainChoices(
                match,
                player)
            .FirstOrDefault(item =>
                item.LoadoutPosition ==
                request.LoadoutPosition);

        if (loadout is null)
            return false;

        BeginCaptainQuestion(match, player, loadout);
        return true;
    }

    internal static void SelectDefaultCaptainQuestion(
        VsMatchSession match)
    {
        var player = FindByPosition(
            match,
            match.Game.CaptainOrder[
                match.Game.CaptainOrderIndex]);
        var loadout = GetCaptainChoices(match, player)
            .OrderBy(item => item.LoadoutPosition)
            .First();

        BeginCaptainQuestion(match, player, loadout);
    }

    internal static bool SubmitGuess(
        VsMatchSession match,
        VsMatchPlayerState? player,
        VsGuessAnswerRequest request,
        DateTime receivedUtc)
    {
        if (match.Phase != VsMatchPhase.NormalRoundGuess ||
            player is null ||
            !player.IsConnected ||
            request.QuestionNumber !=
                match.Game.QuestionNumber ||
            !double.IsFinite(request.Value) ||
            player.CurrentAnswer is not null ||
            IsLate(match, receivedUtc))
        {
            return false;
        }

        player.CurrentAnswer = new VsMatchPlayerAnswerState
        {
            QuestionNumber = request.QuestionNumber,
            Guess = request.Value,
            AnswerTimeSeconds =
                ResolveAnswerTime(match, receivedUtc)
        };

        return true;
    }

    internal static bool SubmitChoice(
        VsMatchSession match,
        VsMatchPlayerState? player,
        VsChoiceAnswerRequest request,
        DateTime receivedUtc)
    {
        if (match.Phase is not
                (VsMatchPhase.NormalRoundQuestion or
                 VsMatchPhase.CaptainQuestion) ||
            player is null ||
            !player.IsConnected ||
            request.QuestionNumber !=
                match.Game.QuestionNumber ||
            request.AnswerIndex is < 0 or > 3 ||
            player.HiddenAnswerIndices.Contains(
                request.AnswerIndex) ||
            player.CurrentAnswer is not null ||
            IsLate(match, receivedUtc))
        {
            return false;
        }

        player.CurrentAnswer = new VsMatchPlayerAnswerState
        {
            QuestionNumber = request.QuestionNumber,
            AnswerIndex = request.AnswerIndex,
            AnswerTimeSeconds =
                ResolveAnswerTime(match, receivedUtc)
        };

        return true;
    }

    internal static bool UseHelp(
        VsMatchSession match,
        VsMatchPlayerState? player,
        VsUseHelpRequest request,
        DateTime receivedUtc)
    {
        if (request.QuestionNumber !=
                match.Game.QuestionNumber ||
            !CanUseHelp(match, player, receivedUtc))
        {
            return false;
        }

        var currentPlayer = player!;
        var round = ResolveCurrentRound(match, currentPlayer);
        var question = match.Game.CurrentQuestion!;

        switch (round.HelpType)
        {
            case VsHelpType.FiftyFifty:
                currentPlayer.HiddenAnswerIndices =
                [
                    .. Enumerable.Range(0, 4)
                        .Where(index =>
                            index !=
                            question.CorrectOptionIndex)
                        .OrderBy(_ => Random.Shared.Next())
                        .Take(2)
                ];
                round.HelpUsed = true;
                break;

            case VsHelpType.TimeFreeze:
                break;

            case VsHelpType.AiSuggestion:
                var accuracy = ResolveHelpValue(
                    currentPlayer,
                    VsHelpType.AiSuggestion);
                var suggestsCorrect =
                    Random.Shared.Next(100) < accuracy;

                currentPlayer.SuggestedAnswerIndex =
                    suggestsCorrect
                        ? question.CorrectOptionIndex
                        : Enumerable.Range(0, 4)
                            .Where(index =>
                                index !=
                                question.CorrectOptionIndex)
                            .OrderBy(_ => Random.Shared.Next())
                            .First();
                round.HelpUsed = true;
                break;

            default:
                return false;
        }

        currentPlayer.ActiveQuestionHelp = round.HelpType;
        return true;
    }

    internal static bool CanUseHelp(
        VsMatchSession match,
        VsMatchPlayerState? player,
        DateTime receivedUtc)
    {
        if (match.Phase is not
                (VsMatchPhase.NormalRoundQuestion or
                 VsMatchPhase.CaptainQuestion) ||
            player is null ||
            !player.IsConnected ||
            player.CurrentAnswer is not null ||
            match.Game.CurrentQuestion is not
                { Kind: VsQuestionKind.Choice } question ||
            question.QuestionerPosition == player.Position ||
            IsLate(match, receivedUtc))
        {
            return false;
        }

        var round = ResolveCurrentRound(match, player);

        return round.HelpType switch
        {
            VsHelpType.FiftyFifty or
            VsHelpType.AiSuggestion =>
                !round.HelpUsed,
            VsHelpType.TimeFreeze =>
                player.ActiveQuestionHelp !=
                VsHelpType.TimeFreeze,
            _ => false
        };
    }

    internal static bool HaveAllParticipantsAnswered(
        VsMatchSession match) =>
        match.Players
            .Where(player => player.IsConnected || player.IsBot)
            .All(player =>
                player.CurrentAnswer?.QuestionNumber ==
                match.Game.QuestionNumber);

    internal static void CloseCurrentQuestion(
        VsMatchSession match)
    {
        if (match.Game.QuestionKind ==
            VsQuestionKind.Guess)
        {
            VsMatchScoring.CloseGuess(match);
            return;
        }

        VsMatchScoring.CloseChoice(match);
    }

    internal static bool HasNextNormalQuestion(
        VsMatchSession match) =>
        match.Game.CurrentQuestionerIndex + 1 <
        match.Game.QuestionerOrder.Length;

    internal static void MoveToNextNormalQuestion(
        VsMatchSession match)
    {
        match.Game.CurrentQuestionerIndex++;
        BeginNormalQuestion(match);
    }

    internal static bool HasNextNormalRound(
        VsMatchSession match) =>
        match.Game.CurrentRoundNumber <
        match.Classification.RequiredPartySize;

    internal static bool HasNextCaptainQuestion(
        VsMatchSession match) =>
        match.Game.CaptainOrderIndex + 1 <
        match.Game.CaptainOrder.Length;

    internal static void MoveToNextCaptainSelection(
        VsMatchSession match)
    {
        match.Game.CaptainOrderIndex++;
        match.Game.QuestionKind = VsQuestionKind.None;
        match.Game.CurrentQuestion = null;
        match.Game.QuestionResult = null;
        ClearAnswers(match);
    }

    internal static void BuildNormalRoundResult(
        VsMatchSession match) =>
        VsMatchScoring.BuildRoundResult(
            match,
            isCaptainRound: false);

    internal static void BuildCaptainRoundResult(
        VsMatchSession match) =>
        VsMatchScoring.BuildRoundResult(
            match,
            isCaptainRound: true);

    internal static void CommitRoundResult(
        VsMatchSession match) =>
        VsMatchScoring.CommitRoundResult(match);

    internal static VsMatchLoadoutItemState[] GetCaptainChoices(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        var normalRoundPositions = player.Rounds
            .Where(round => !round.IsCaptainRound)
            .Select(round => round.LoadoutPosition)
            .Where(position => position.HasValue)
            .Select(position => position!.Value)
            .ToHashSet();

        return
        [
            .. player.Loadout
                .Where(item =>
                    !normalRoundPositions.Contains(
                        item.LoadoutPosition) &&
                    !player.CaptainUsedLoadoutPositions.Contains(
                        item.LoadoutPosition))
                .OrderBy(item => item.LoadoutPosition)
        ];
    }

    private static void BeginNormalRound(
        VsMatchSession match)
    {
        ResetRoundState(match);
        ClearAnswers(match);

        var guess = match.GuessQuestions[
            match.Game.CurrentRoundNumber - 1];

        match.Game.QuestionNumber++;
        match.Game.QuestionKind = VsQuestionKind.Guess;
        match.Game.CurrentQuestion = new VsMatchQuestionState
        {
            Kind = VsQuestionKind.Guess,
            Question = guess.Question,
            CorrectGuess = guess.CorrectAnswer
        };
        match.Game.QuestionerOrder = [];
        match.Game.CurrentQuestionerIndex = 0;
        match.Game.QuestionResult = null;
        match.Game.RoundResult = [];

        BuildGuessRanges(match);
    }

    private static void BeginCaptainQuestion(
        VsMatchSession match,
        VsMatchPlayerState player,
        VsMatchLoadoutItemState loadout)
    {
        player.CaptainUsedLoadoutPositions.Add(
            loadout.LoadoutPosition);
        ClearAnswers(match);

        match.Game.QuestionNumber++;
        match.Game.QuestionKind = VsQuestionKind.Choice;
        match.Game.CurrentQuestion =
            CreateChoiceQuestion(
                loadout,
                player.Position);
        match.Game.QuestionResult = null;
    }

    private static VsMatchQuestionState CreateChoiceQuestion(
        VsMatchLoadoutItemState loadout,
        int questionerPosition) =>
        new()
        {
            Kind = VsQuestionKind.Choice,
            Question = loadout.Question,
            Answers = loadout.Answers,
            CorrectOptionIndex =
                loadout.CorrectOptionIndex,
            QuestionerPosition = questionerPosition,
            CategoryId = loadout.QuestionCategoryId,
            QuestionId = loadout.QuestionId,
            IsOwnQuestion = loadout.IsOwnQuestion
        };

    private static void ResetRoundState(VsMatchSession match)
    {
        foreach (var player in match.Players)
        {
            player.RoundPoints = 0;
            player.RoundTimeSeconds = 0;
            player.CurrentAnswer = null;
            player.RoundProgress.Clear();
        }
    }

    private static void ClearAnswers(VsMatchSession match)
    {
        foreach (var player in match.Players)
        {
            player.CurrentAnswer = null;
            player.ActiveQuestionHelp = VsHelpType.None;
            player.GuessRangeMinimum = null;
            player.GuessRangeMaximum = null;
            player.HiddenAnswerIndices = [];
            player.SuggestedAnswerIndex = null;
        }
    }

    private static void BuildGuessRanges(VsMatchSession match)
    {
        var correctGuess =
            match.Game.CurrentQuestion!.CorrectGuess;

        foreach (var player in match.Players)
        {
            var round = ResolveCurrentRound(match, player);

            if (round.HelpType != VsHelpType.GuessRange)
                continue;

            var errorPercent = ResolveHelpValue(
                player,
                VsHelpType.GuessRange);
            var width = Math.Max(
                1,
                Math.Ceiling(
                    Math.Abs(correctGuess) *
                    errorPercent /
                    100));
            var minimum = Math.Floor(
                correctGuess -
                Random.Shared.NextDouble() * width);

            if (minimum < 1)
                minimum = 1;

            player.GuessRangeMinimum = minimum;
            player.GuessRangeMaximum =
                Math.Ceiling(minimum + width);
        }
    }

    private static double ResolveHelpValue(
        VsMatchPlayerState player,
        VsHelpType helpType)
    {
        var helpIndex = (int)helpType - 1;
        var level = player.HelpLevels[helpIndex];

        return ModifierTable.Data[level]
                   .Modifier[helpIndex + 8] ??
               0;
    }

    private static VsMatchRoundState ResolveCurrentRound(
        VsMatchSession match,
        VsMatchPlayerState player) =>
        player.Rounds.First(round =>
            round.RoundNumber ==
            match.Game.CurrentRoundNumber);

    private static VsMatchPlayerState FindByPosition(
        VsMatchSession match,
        int position) =>
        match.Players.First(player =>
            player.Position == position);

    private static bool IsLate(
        VsMatchSession match,
        DateTime receivedUtc) =>
        match.DeadlineUtc.HasValue &&
        receivedUtc > match.DeadlineUtc.Value;

    private static double ResolveAnswerTime(
        VsMatchSession match,
        DateTime receivedUtc) =>
        Math.Max(
            0,
            (receivedUtc - match.PhaseStartedUtc)
            .TotalSeconds);
}

/**
 * ÚJ FÁJL: a VS játékmenet explicit állapotátmeneteit és a három
 * publikus parancs szerveroldali validálását tartalmazza. Nem kezel
 * hálózatot, időzítőt vagy adatbázist; a válasz kizárólag az aktuális
 * fázis és QuestionNumber esetén kerül a sessionbe.
 * MÓDOSÍTÁS: a játékkérdés a loadout megjelenítési kategóriája
 * helyett a tényleges kérdéskategóriát kapja, így az „összes”
 * választás időmódosítója is ugyanúgy működik. Az eredeti kérdés-
 * azonosító és a sajátkérdés-jelző a statisztikához szintén megmarad.
 * MÓDOSÍTÁS: szerveroldalon aktiválja és validálja a 50-50,
 * időtlenítő és AI segítséget. A tippsávot a nagy kör elején
 * automatikusan építi fel; a help-szint százaléka közvetlenül a
 * teljes sáv helyes válaszhoz viszonyított szélességét adja.
 * MÓDOSÍTÁS: egy kérdés akkor zárható korábban, ha minden aktív
 * ember vagy szerveroldali bot válaszolt; a lecsatlakozott bot nem
 * rövidíti le tévesen a többiek kérdésidejét.
 */
