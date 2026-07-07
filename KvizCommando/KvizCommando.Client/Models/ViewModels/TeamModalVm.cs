using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace KvizCommando.Client.Models.ViewModels
{
    public abstract class TeamModalVm
    {
        public List<ModalRow> Rows { get; set; } = new();
        public InfoBlock Info { get; set; } = default!;
        public string Infotext1 { get; set; } = string.Empty;

    }
    public sealed record ModalRow(
        string CategoryName,
        string ValueDisplay,
        string separator,
        string ValueChangeDisplay,
        string color
        );
    public sealed record InfoBlock(
        string Name,
        string NameValue,
        string Color,
        string Rank,
        string RankValue,
        string Level,
        string LevelValue,
        string Orient1,
        string Orient2,
        string Orient1Value,
        string Orient2Value,
        string Devpoints,
        string DevPointsValue,
        string AddedDevPoints
        );
    public sealed class ModalHireVm : TeamModalVm
    {
        public string Labelpros { get; set; } = string.Empty;
        public string Labelcons { get; set; } = string.Empty;
    }
    public class ModalRetireVm : TeamModalVm
    {
        public string Unlocks { get; set; } = string.Empty;
        public string UnlocksLevel { get; set; } = string.Empty;
        public string UnlocksRank { get; set; } = string.Empty;
        public string RankClass { get; set; } = string.Empty;
        public bool RankClassChanged { get; set; } = false;
    }
    public sealed class ModalPromoteVm: ModalRetireVm
    {
        
        public string UnlockMaxLevels1 { get; set; } = string.Empty;
        public string UnlockMaxLevels2 { get; set; } = string.Empty;
    }
    public sealed class ModalHandleVm : TeamModalVm
    {
        
        public string Infotext2 { get; set;} =string.Empty;
        public string Infotext3 { get; set;} = string.Empty;
        public string Infotext4 { get; set; } = string.Empty;
    }
    public static class ModalConstants
    {
        public static readonly int[] HireVal = 
        { 
           0, 0, 4,0, 1, 5 
        };
       
    }
}
