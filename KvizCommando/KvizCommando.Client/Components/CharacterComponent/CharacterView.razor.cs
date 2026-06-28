using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace KvizCommando.Client.Components.CharacterComponent
{
    public partial class CharacterView : IDisposable
    {
        
        [Parameter] public string PicCode { get; set; } = string.Empty;

        private readonly static string[] colEye = new string[]
       {
            "#000080", // kék
            "#225500", // zöld
            "#552200", // barna
            "#2b1100", // fekete
       };
        private readonly static string[] colHair = new string[]
        {
            "#ffeeaa", // Szőke
            "#ffcc00", // aranyszőke
            "#ff6600", // vörös
            "#aa4400", // világosbarna
            "#803300", // barna	
            "#552200", //sötétbarna
            "#241c1c", //fekete
            "#E3e2db" // ősz
        };
        private readonly static string[] colSkin = new string[]
        {
            "#f4e3d7", // fal fehér
            "#e9c6af", // nagyon fehér
            "#deaa87", // fehér
            "#d38d5f", // cigány
            "#c87137", // latino		
            "#a05a2c", // arab
            "#784421", // afro-americano
            "#502d16" // nigger
        };
        private CharacterModel _character=new(); 
        protected override void OnParametersSet()
        {
            _character = BuildCharacterFromPicCode(PicCode);
        }

        private CharacterModel BuildCharacterFromPicCode(string piccode)
        {
            // M 1 2 3 4 A G
            // F 1 E3 H4 A G
            // M 8 E4 H0 B N
            //piccode = CharacterGenerator();
            if (piccode == "") return new CharacterModel() { genre = null };
            var skinIndex = Math.Min(int.Parse(piccode[1].ToString()),7);
            var eyeIndex = Math.Min(int.Parse(piccode[2].ToString()),3);
            var hairIndex = Math.Min(int.Parse(piccode[3].ToString()),7);
            var hairstyle = Math.Min(int.Parse(piccode[4].ToString()),7);
            return new CharacterModel() {
                skinColor = colSkin[skinIndex],
                eyeColor = colEye[eyeIndex],
                hairColor = colHair[hairIndex],
                hairStyle = (byte)hairstyle,
                asian = piccode[5] == 'A' ? true : false,
                glasses = piccode[6] == 'G' ? true : false,
                genre = piccode[0] == 'F' ? true : false
            };
        }
       
        public void Dispose()
        {
           
            GC.SuppressFinalize(this);
        }
    }
}
