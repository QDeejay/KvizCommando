using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class UpperBlockDisplay
    {
        [Inject] private UpperBlockDataBuilder UpperBuilder { get; set; } = default!;
        [Parameter] public IGeneralInfo DatatoProc { get; set; } = default!;
        [Parameter] public int UsedSkillPoints { get; set; } = 0;

        private UpperBlockViewModel vm = new();
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        protected override void OnParametersSet()
        {
            vm = UpperBuilder.Build(DatatoProc, UsedSkillPoints, culture);
        }
    }
}
