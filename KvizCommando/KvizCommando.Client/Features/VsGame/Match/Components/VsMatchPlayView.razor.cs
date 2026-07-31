using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace KvizCommando.Client.Features.VsGame.Match.Components;

public partial class VsMatchPlayView : IDisposable
{
    [Inject] private ILanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public VsMatchViewData Data { get; set; } = new();

    [Parameter]
    public EventCallback<VsGuessAnswerRequest>
        OnGuessSubmitted { get; set; }

    [Parameter]
    public EventCallback<VsChoiceAnswerRequest>
        OnChoiceSubmitted { get; set; }

    [Parameter]
    public EventCallback<VsUseHelpRequest>
        OnHelpUsed { get; set; }

    [Parameter]
    public EventCallback<VsCaptainQuestionRequest>
        OnCaptainQuestionSelected { get; set; }

    private System.Threading.Timer? _timer;
    private ElementReference _guessInput;
    private string _guessText = string.Empty;
    private int _lastQuestionNumber;
    private int _focusedGuessQuestionNumber = -1;
    private bool _sending;

    protected override void OnInitialized()
    {
        _timer = new System.Threading.Timer(
            _ => _ = InvokeAsync(StateHasChanged),
            null,
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250));
    }

    protected override void OnParametersSet()
    {
        if (_lastQuestionNumber ==
            Data.Game.QuestionNumber)
        {
            return;
        }

        _lastQuestionNumber =
            Data.Game.QuestionNumber;
        _guessText = string.Empty;
        _sending = false;
    }

    protected override async Task OnAfterRenderAsync(
        bool firstRender)
    {
        if (Data.Game.QuestionKind != VsQuestionKind.Guess ||
            !IsAnswerTimeActive ||
            _focusedGuessQuestionNumber ==
                Data.Game.QuestionNumber)
        {
            return;
        }

        await _guessInput.FocusAsync();

        _focusedGuessQuestionNumber =
            Data.Game.QuestionNumber;
    }

    private int RemainingSeconds => !Data.DeadlineUtc.HasValue
        ? 0
        : Math.Max(
            0,
            (int)Math.Ceiling(
                (Data.DeadlineUtc.Value - DateTime.UtcNow)
                .TotalSeconds));

    private int DisplaySeconds =>
        Data.Phase == VsMatchPhase.CaptainQuestionSelection &&
        Data.DeadlineUtc.HasValue
            ? Math.Max(
                0,
                (int)Math.Round(
                    (Data.DeadlineUtc.Value - DateTime.UtcNow)
                    .TotalSeconds,
                    MidpointRounding.AwayFromZero))
            : RemainingSeconds;

    private bool HasTimeRemaining =>
        Data.DeadlineUtc.HasValue &&
        DateTime.UtcNow < Data.DeadlineUtc.Value;

    private double TimerPercent => Data.PhaseDurationSeconds <= 0
        ? 0
        : Math.Clamp(
            (double)RemainingSeconds /
            Data.PhaseDurationSeconds * 100,
            0,
            100);

    private string TimerStyle =>
        $"width: {TimerPercent.ToString(
            "0.##",
            CultureInfo.InvariantCulture)}%";

    private int WarningSeconds =>
        Data.Phase == VsMatchPhase.CaptainQuestionSelection
            ? 3
            : 5;

    private string TimerStateClass => DisplaySeconds switch
    {
        <= 0 => "zero",
        _ when DisplaySeconds <= WarningSeconds => "warning",
        _ => string.Empty
    };

    private string WaitTimerStyle =>
        $"--kc-vs-wait-progress: {TimerPercent.ToString(
            "0.##",
            CultureInfo.InvariantCulture)}%";

    private bool ShowAnswerTimer =>
        Data.DeadlineUtc.HasValue &&
        Data.Phase is
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestionSelection or
            VsMatchPhase.CaptainQuestion;

    private bool ShowWaitTimer =>
        Data.DeadlineUtc.HasValue &&
        Data.Phase is
            VsMatchPhase.PreparationStarting or
            VsMatchPhase.GameStarting or
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainRoundResult;

    private bool ShowProgress =>
        Data.Game.CurrentRoundNumber > 0 &&
        Data.Phase != VsMatchPhase.GameCompleted;

    private string MatchReference =>
        Data.MatchId.ToString("N")[..8]
            .ToUpperInvariant();

    private string RoundText =>
        Data.Game.CurrentRoundNumber <= 0
            ? Data.ClassificationText
            : Data.Game.CurrentRoundNumber >
        Data.Game.NormalRoundCount
            ? Lang["vsgame.Match.Round.Captain"]
            : Lang["vsgame.Match.Round.Normal"]
                .FormatSafe(
                    Data.Game.CurrentRoundNumber);

    private bool CanSubmitGuess =>
        IsAnswerTimeActive &&
        !IsGuessLocked &&
        TryGetGuessValue(out _) &&
        !_sending;

    private bool IsAnswerTimeActive =>
        Data.Game.CanAnswer &&
        HasTimeRemaining;

    private bool CanUseHelpNow =>
        Data.Game.CanUseHelp &&
        HasTimeRemaining;

    private bool ShowPlayerChoices =>
        Data.Game.QuestionKind == VsQuestionKind.Choice &&
        (Data.Phase == VsMatchPhase.QuestionResult ||
         Data.Game.MyAnswerIndex.HasValue);

    private VsHelpCardVm? CurrentHelp =>
        Data.Preparation.Rounds
            .FirstOrDefault(round =>
                round.RoundNumber ==
                Data.Game.CurrentRoundNumber)
            ?.Help;

    private bool HasGuessRange =>
        Data.Game.MyGuessRangeMinimum.HasValue &&
        Data.Game.MyGuessRangeMaximum.HasValue;

    private bool IsGuessLocked =>
        Data.Game.MyGuess.HasValue ||
        Data.Game.CorrectGuess.HasValue;

    private string GuessInputClass =>
        Data.Game.CorrectGuess.HasValue
            ? "game-display"
            : Data.Game.MyGuess.HasValue
                ? "game-display submitted"
                : string.Empty;

    private string GuessInputText
    {
        get => Data.Game.CorrectGuess.HasValue
            ? $"{Lang["vsgame.Match.Game.CorrectGuess"]}: " +
              FormatNumber(Data.Game.CorrectGuess.Value)
            : Data.Game.MyGuess.HasValue
                ? FormatNumber(Data.Game.MyGuess.Value)
                : _guessText;
        set
        {
            if (!IsGuessLocked)
                _guessText = value;
        }
    }

    private string GuessRangeText =>
        $"({FormatNumber(Data.Game.MyGuessRangeMinimum!.Value)} - " +
        $"{FormatNumber(Data.Game.MyGuessRangeMaximum!.Value)})";

    private string HelpUseMarker =>
        Data.Game.IsMyHelpUnlimited
            ? "∞"
            : Data.Game.MyHelpUsesRemaining.ToString();

    private int MyRoundPoints =>
        Data.Game.MyRoundPoints;

    private double MyRoundTime =>
        Data.Game.MyRoundTimeSeconds;

    private int MyPosition =>
        Data.Players.First(player => player.IsMe).Position;

    private string PlayerName(int position) =>
        Data.Players
            .FirstOrDefault(player =>
                player.Position == position)
            ?.DisplayName ?? position.ToString();

    private static string Signed(int value) =>
        value > 0 ? $"+{value}" : value.ToString();

    private static string Seconds(double value) =>
        $"{value:0.0}s";

    private static string FormatNumber(double value) =>
        value.ToString(
            "0.##",
            CultureInfo.CurrentCulture);

    private string GuessText(VsQuestionPlayerVm player)
    {
        if (Data.Game.CorrectGuess.HasValue)
        {
            return player.Guess.HasValue
                ? FormatNumber(player.Guess.Value)
                : "—";
        }

        return player.Position == MyPosition &&
               Data.Game.MyGuess.HasValue
            ? FormatNumber(Data.Game.MyGuess.Value)
            : "•••";
    }

    private bool IsGuessRevealed(VsQuestionPlayerVm player) =>
        Data.Game.CorrectGuess.HasValue ||
        (player.Position == MyPosition &&
         Data.Game.MyGuess.HasValue);

    private bool IsAnswerEliminated(int answerIndex) =>
        !Data.Game.CorrectAnswerIndex.HasValue &&
        Data.Game.MyHiddenAnswerIndices.Contains(answerIndex);

    private bool IsSuggestedAnswer(int answerIndex) =>
        !Data.Game.CorrectAnswerIndex.HasValue &&
        Data.Game.MySuggestedAnswerIndex == answerIndex;

    private IEnumerable<VsQuestionPlayerVm> PlayersOnAnswer(
        int answerIndex)
    {
        if (Data.Phase == VsMatchPhase.QuestionResult)
        {
            return Data.Game.QuestionPlayers
                .Where(player =>
                    player.AnswerIndex == answerIndex)
                .OrderBy(player => player.Position);
        }

        if (Data.Game.MyAnswerIndex != answerIndex)
            return [];

        return Data.Game.QuestionPlayers
            .Where(player =>
                player.Position == MyPosition);
    }

    private static string PlayerToneClass(int position) =>
        $"player-tone-{position}";

    private bool IsQuestioner(int position) =>
        Data.Game.QuestionerPosition == position;

    private string AnswerClass(int answerIndex)
    {
        string css;

        if (!Data.Game.CorrectAnswerIndex.HasValue)
        {
            css = Data.Game.MyAnswerIndex == answerIndex
                ? "selected"
                : string.Empty;
        }
        else if (Data.Game.CorrectAnswerIndex == answerIndex)
        {
            css = "correct";
        }
        else
        {
            css = Data.Game.MyAnswerIndex == answerIndex
                ? "wrong"
                : string.Empty;

            css = $"{css} result-muted".Trim();
        }

        if (IsAnswerEliminated(answerIndex))
            css = $"{css} eliminated".Trim();

        if (IsSuggestedAnswer(answerIndex))
            css = $"{css} suggested".Trim();

        if (!Data.Game.CorrectAnswerIndex.HasValue &&
            !IsAnswerTimeActive &&
            Data.Game.MyAnswerIndex != answerIndex)
        {
            css = $"{css} locked-muted".Trim();
        }

        return css;
    }

    private async Task SubmitGuessAsync()
    {
        if (!CanSubmitGuess ||
            !TryGetGuessValue(out var guessValue))
        {
            return;
        }

        _sending = true;

        try
        {
            await OnGuessSubmitted.InvokeAsync(
                new VsGuessAnswerRequest
                {
                    QuestionNumber =
                        Data.Game.QuestionNumber,
                    Value = guessValue
                });
        }
        finally
        {
            _sending = false;
        }
    }

    private Task HandleGuessKeyDownAsync(
        KeyboardEventArgs args) =>
        args.Key == "Enter"
            ? SubmitGuessAsync()
            : Task.CompletedTask;

    private bool TryGetGuessValue(
        out double value) =>
        double.TryParse(
            _guessText,
            NumberStyles.Float,
            CultureInfo.CurrentCulture,
            out value) &&
        double.IsFinite(value);

    private async Task SubmitChoiceAsync(int answerIndex)
    {
        if (!IsAnswerTimeActive ||
            IsAnswerEliminated(answerIndex) ||
            _sending)
        {
            return;
        }

        _sending = true;

        try
        {
            await OnChoiceSubmitted.InvokeAsync(
                new VsChoiceAnswerRequest
                {
                    QuestionNumber =
                        Data.Game.QuestionNumber,
                    AnswerIndex = answerIndex
                });
        }
        finally
        {
            _sending = false;
        }
    }

    private async Task UseHelpAsync()
    {
        if (!CanUseHelpNow || _sending)
            return;

        _sending = true;

        try
        {
            await OnHelpUsed.InvokeAsync(
                new VsUseHelpRequest
                {
                    QuestionNumber =
                        Data.Game.QuestionNumber
                });
        }
        finally
        {
            _sending = false;
        }
    }

    private async Task SelectCaptainQuestionAsync(
        int loadoutPosition)
    {
        if (!Data.Game.CanChooseCaptainQuestion ||
            _sending)
        {
            return;
        }

        _sending = true;

        try
        {
            await OnCaptainQuestionSelected.InvokeAsync(
                new VsCaptainQuestionRequest
                {
                    LoadoutPosition = loadoutPosition
                });
        }
        finally
        {
            _sending = false;
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/**
 * ÚJ FÁJL: a VS játéknézet visszaszámlálását, lokális beviteli
 * állapotát és az explicit EventCallback parancsokat kezeli.
 * Pontot, sorrendet vagy válaszjogosultságot nem számol kliensoldalon.
 * A teljes MatchId-ből csak megjelenítési célú rövid hivatkozást képez.
 * MÓDOSÍTÁS: külön kezeli a válaszadási idősort és a színezés nélküli,
 * kör alakú várakozási visszaszámlálót.
 * MÓDOSÍTÁS: Enter esetén ugyanazt az ellenőrzött tippbeküldő
 * handlert hívja, mint a megjelenített BI gomb.
 * MÓDOSÍTÁS: a snapshot szerinti tippsávot, 50-50 kizárást és
 * felülírható AI-javaslatot jeleníti meg; a segítség gombja
 * kizárólag az explicit UseHelp EventCallbackot hívja.
 * MÓDOSÍTÁS: a tippkérdés inputját kérdésenként egyszer, közvetlenül
 * render után Blazor ElementReference segítségével fókuszálja.
 * A részben begépelt értéket szövegként őrzi, és csak a közös
 * beküldési útvonalon alakítja véges double értékké.
 * MÓDOSÍTÁS: a szerveres deadline elérésekor azonnal lezárja a
 * kliensinterakciókat, a számlálót nullán tartja a felfedési
 * késleltetés alatt, és külön warning/zero megjelenítést ad.
 * MÓDOSÍTÁS: lezáráskor halványítja a nem választott válaszokat,
 * eredménykor pedig pozíció szerinti játékosszínnel jeleníti meg,
 * hogy az egyes válaszokat kik jelölték.
 * MÓDOSÍTÁS: a tippfelfedéshez a snapshotban már meglévő játékos-
 * tippeket használja; a kérdező pozícióját csak vizuális osztályhoz
 * hasonlítja össze, pontot vagy jogosultságot továbbra sem számol.
 * MÓDOSÍTÁS: eredmény előtt a tippérték helyén semleges jelölést ad,
 * a saját választ pedig a MyAnswerIndex és a meglévő saját roster-
 * pozíció alapján ugyanazzal a színnel jeleníti meg. Más játékos
 * válaszát továbbra sem következteti ki és nem fedi fel kliensoldalon.
 * MÓDOSÍTÁS: a saját tipp szövegét és felfedési állapotát közvetlenül
 * a személyre szabott MyGuess snapshotmezőből adja; a többiek tippje
 * a közös eredményig változatlanul rejtett marad.
 * MÓDOSÍTÁS: ugyanaz a bindolt tippmező mutatja a beírt, a szerver
 * által visszaigazolt, majd a felfedett helyes értéket is.
 * A felfedett érték elé visszakerül a lokalizált „Helyes érték” címke.
 * MÓDOSÍTÁS: a kérdéseredmény rövid szünete számláló nélkül telik;
 * a kapitányi kérdésválasztás kijelzője a 0 értéket is megmutatja.
 */
