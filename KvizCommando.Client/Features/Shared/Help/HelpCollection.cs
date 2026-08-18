using KvizCommando.Client.Features.Shared.Help.QuestionRules;
using KvizCommando.Client.Features.Shared.Help.SoloRules;
using KvizCommando.Client.Features.Shared.Help.TeamHelpRules;
using KvizCommando.Client.Features.Shared.Help.VsRules;
using KvizCommando.Client.Features.Question.Builders;
using KvizCommando.Client.Features.Solo.Builders;
using KvizCommando.Client.Features.Team.Builders;
using KvizCommando.Client.Features.VsGame.Builders;
using KvizCommando.Client.Pages.Home.Features;
using KvizCommando.Client.Services.ClientCache;

namespace KvizCommando.Client.Features.Shared.Help;

internal sealed class HelpPackage
{
    internal string[] Paths { get; }
    internal Func<AppState, IReadOnlyDictionary<string, string>> BuildTokens { get; }
    internal required HomeBoxKey Root { get; init; }
    internal required string TitleKey { get; init; }
    internal required string BackgroundImage { get; init; }

    internal HelpPackage(
        string[] paths,
        Func<AppState, IReadOnlyDictionary<string, string>> buildTokens)
    {
        Paths = paths;
        BuildTokens = buildTokens;
    }
}

public static class HelpCollection
{
    public const string SEEN_STORAGE_KEY = "SeenHelps";
    public const string LANDING_PATH = "index.html";
    public const string LANDING_BACKGROUND = "/images/logo.webp";

    internal static IReadOnlyDictionary<int, HelpPackage> Packages { get; } =
        new Dictionary<int, HelpPackage>
        {
            [(int)QBoxKeyRoot.Factory] = new(
                [
                    "question/loadout-01.html",
                    "question/loadout-02.html",
                    "question/loadout-03.html"
                ],
                _ => LoadoutHelpRules.Tokens)
            {
                Root = HomeBoxKey.Question,
                TitleKey = "home.SubBox.Title.Question.Factory",
                BackgroundImage = "/images/buttons/question/fact.webp"
            },
            [(int)QBoxKeyRoot.Usr] = new(
                [
                    "question/user-questions-01.html",
                    "question/user-questions-02.html"
                ],
                _ => UserPendingHelpRules.Tokens)
            {
                Root = HomeBoxKey.Question,
                TitleKey = "home.SubBox.Title.Question.Usr",
                BackgroundImage = "/images/buttons/question/usr.webp"
            },
            [(int)QBoxKeyRoot.Pending] = new(
                [
                    "question/pending-questions-01.html",
                    "question/pending-questions-02.html"
                ],
                _ => UserPendingHelpRules.Tokens)
            {
                Root = HomeBoxKey.Question,
                TitleKey = "home.SubBox.Title.Question.Pending",
                BackgroundImage = "/images/buttons/question/pending.webp"
            },
            [(int)QBoxKeyRoot.New] = new(
                [
                    "question/new-question-01.html",
                    "question/new-question-02.html"
                ],
                _ => NewQuestionHelpRules.Tokens)
            {
                Root = HomeBoxKey.Question,
                TitleKey = "home.SubBox.Title.Question.New",
                BackgroundImage = "/images/buttons/question/new.webp"
            },
            [(int)TBoxKeyRoot.TeamOverview] = new(
                [
                    "team/team-overview-01.html",
                    "team/team-overview-02.html",
                    "team/team-overview-03.html",
                    "team/team-overview-04.html",
                    "team/team-overview-05.html",
                    "team/team-overview-06.html"
                ],
                TeamOverviewHelpRules.BuildTokens)
            {
                Root = HomeBoxKey.Team,
                TitleKey = "home.SubBox.Title.Team.TeamOverview",
                BackgroundImage = "/images/buttons/team/team.webp"
            },
            [(int)TBoxKeyRoot.Members] = new(
                [
                    "team/member-01.html",
                    "team/member-02.html",
                    "team/member-03.html",
                    "team/member-04.html",
                    "team/member-05.html"
                ],
                MemberHelpRules.BuildTokens)
            {
                Root = HomeBoxKey.Team,
                TitleKey = "home.SubBox.Title.Team.Members",
                BackgroundImage = "/images/buttons/team/members.webp"
            },
            [(int)TBoxKeyRoot.Recruit] = new(
                [
                    "team/recruit-01.html",
                    "team/recruit-02.html",
                    "team/recruit-03.html",
                    "team/recruit-04.html",
                    "team/recruit-05.html"
                ],
                RecruitHelpRules.BuildTokens)
            {
                Root = HomeBoxKey.Team,
                TitleKey = "home.SubBox.Title.Team.Recruit",
                BackgroundImage = "/images/buttons/team/recruit.webp"
            },
            [(int)VsBoxKeyRoot.RankedBattlefields] = new(
                [
                    "vsgame/ranked-01.html",
                    "vsgame/ranked-02.html",
                    "vsgame/ranked-03.html",
                    "vsgame/ranked-04.html",
                    "vsgame/ranked-05.html",
                    "vsgame/ranked-06.html"
                ],
                VsRankedHelpRules.BuildTokens)
            {
                Root = HomeBoxKey.GameVs,
                TitleKey = "home.SubBox.Title.GameVs.RankedBattlefields",
                BackgroundImage = "/images/buttons/vsgame/ranked.webp"
            },
            [(int)SgameBoxKeyRoot.Category] = new(
                [
                    "solo/category-01.html",
                    "solo/category-02.html",
                    "solo/category-03.html",
                    "solo/category-04.html",
                    "solo/category-05.html",
                    "solo/category-06.html"
                ],
                _ => SoloCategoryHelpRules.Tokens)
            {
                Root = HomeBoxKey.GameSolo,
                TitleKey = "home.SubBox.Title.GameSolo.Category",
                BackgroundImage = "/images/buttons/solo/categories.webp"
            },
            [(int)SgameBoxKeyRoot.Orientation] = new(
                [
                    "solo/orientation-01.html",
                    "solo/orientation-02.html",
                    "solo/orientation-03.html",
                    "solo/orientation-04.html",
                    "solo/orientation-05.html"
                ],
                _ => SoloOrientationHelpRules.Tokens)
            {
                Root = HomeBoxKey.GameSolo,
                TitleKey = "home.SubBox.Title.GameSolo.Orientation",
                BackgroundImage = "/images/buttons/solo/orients.webp"
            }
        };
}
