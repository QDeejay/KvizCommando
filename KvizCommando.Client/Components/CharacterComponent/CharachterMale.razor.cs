using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Components.CharacterComponent
{
    public partial class CharachterMale
    {
        [Parameter] public string HairColor { get; set; } = "#000000";
        [Parameter] public string SkinColor { get; set; } = "#ffffff";
        [Parameter] public string EyeColor { get; set; } = "#000000";
        [Parameter] public byte Hairstyle { get; set; } = 0;

        [Parameter] public bool Glasses { get; set; } = false;
        [Parameter] public bool Asian { get; set; } = false;

        private const string inline = "inline";
        private const string none = "none";
        private string _hairColor => HairColor;
        private string _skinColor => SkinColor;
        private string _eyeColor => EyeColor;
        private string  _glasses => Glasses == true ? inline : none;
        private string  _asian => Asian == true ? inline : none;
        private string _nonasian => Asian == false ? inline : none;
        private string  _hairOption1 => (Hairstyle & (1 << 0)) != 0 ? inline : none;
        private string _hairOption2 => (Hairstyle & (1 << 1)) != 0 ? inline : none;
        private string _hairOption3 => (Hairstyle & (1 << 2)) != 0 ? inline : none;

    }
}
