using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class BottomBlockViaButtons
    {
        //[Inject] private DevDataBuilder DevBuilder { get; set; } = default!;
        //[Parameter] public IGeneralInfo DatatoProc { get; set; } = default!;
        [Parameter] public HelpDto DataHelp { get; set; } = default!;
        [Parameter] public int DevPointsUsed { get; set; }
        [Parameter] public EventCallback<ModifySkillRequest> ModifySkill { get; set; } = default!;
        [Parameter] public int TabPosV { get; set; } = default!;
        [Parameter] public int TabPosH { get; set; } = default!;

        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private BottomDevVm vm = new();
        private int[] usedPoints { get; set; } = default!;
        private int _prevTabPosV = -1;
        private int _prevTabPosH = -1;
        private bool _saved = false;
        

        protected override void OnParametersSet()
        {
            if (TabPosV != _prevTabPosV)
            {
                ResetUsedPoints();
                _prevTabPosV = TabPosV;
            }
            if (TabPosH != _prevTabPosH)
            {
                ResetUsedPoints();
                _prevTabPosH = TabPosH;
            }
            if (_saved)
            {
                _saved = false;
                ResetUsedPoints();
            }
            else
            {
                if (TabPosV == 1 || TabPosV == 2)
                    Console.WriteLine("");
                //vm = DevBuilder(TabPosV, DatatoProc, usedPoints, DataHelp, culture);
            }
        }
        protected void OnIncButtonPushed(int rowId)
        {
            int[] usdPnts = usedPoints;
            //if (usdPnts.Sum() >= DatatoProc.DevPts || rowId == 0) return;
            usedPoints[rowId - 1]++;
            usdPnts = usedPoints;
            //vm = DevBuilder.Build(TabPosV, DatatoProc, usdPnts, DataHelp, culture);
        }
        protected void OnDecButtonPushed(int rowId)
        {
            int[] usdPnts = usedPoints;
           // if (DatatoProc.DevPts == 0 || rowId == 0) return;
            if (usedPoints[rowId - 1] > 0) usedPoints[rowId - 1]--;
            usdPnts = usedPoints;
            //vm = DevBuilder.Build(TabPosV, DatatoProc, usdPnts, DataHelp, culture);
        }
        protected async Task OnSaveButtonPushed()
        {
            if (usedPoints.Sum() == 0 || (TabPosV != 1 && TabPosV != 2) || TabPosH > 8) return;
            ModifySkillRequest request = new()
            {
                SkillChanges = usedPoints,
                SkillType = TabPosV,
                MemberId = TabPosH
            };
            _saved = true;
            if (ModifySkill.HasDelegate)
                await ModifySkill.InvokeAsync(request);

        }
        private void ResetUsedPoints()
        {
            usedPoints = [0, 0, 0, 0];
        }
        public void Dispose()
        {
            ModifySkill = default;
            GC.SuppressFinalize(this);
        }
    }
}
