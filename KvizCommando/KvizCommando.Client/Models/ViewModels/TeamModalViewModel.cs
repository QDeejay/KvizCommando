using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace KvizCommando.Client.Models.ViewModels
{
    public abstract class TeamModalViewModel
    {
        public List<ModalRow> Rows { get; set; } = new();
        public InfoBlock Info { get; set; }
        public string infotext1 { get; set; } = string.Empty;

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

    public sealed class HireModalViewModel : TeamModalViewModel
    {
        public string labelpros { get; set; } = string.Empty;
        public string labelcons { get; set; } = string.Empty;
    }
    public class RetireModalView : TeamModalViewModel
    {
        public string Unlocks { get; set; } = string.Empty;
        public string UnlocksLevel { get; set; } = string.Empty;
        public string UnlocksRank { get; set; } = string.Empty;
        public string RankClass { get; set; } = string.Empty;
        public bool RankClassChanged { get; set; } = false;
    }
    public sealed class PromoteModalView : RetireModalView
    {
        
        public string UnlockMaxLevels1 { get; set; } = string.Empty;
        public string UnlockMaxLevels2 { get; set; } = string.Empty;
    }
    public sealed class HandleModalView : TeamModalViewModel
    {
        
        public string infotext2 { get; set;} =string.Empty;
        public string infotext3 { get; set;} = string.Empty;
        public string infotext4 { get; set; } = string.Empty;
    }
    public static class ModalConstants
    {
        public static readonly int[] HireVal = 
        { 
           0, 0, 4,0, 1, 5 
        };
       
    }
}
