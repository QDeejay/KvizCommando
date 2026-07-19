using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Components.CharacterComponent
{
    public partial class CharacterView : IDisposable
    {

        [Parameter] public string PicCode { get; set; } = string.Empty;

        private readonly static string[] colEye =
        [
            "#000080", // kék
            "#225500", // zöld
            "#552200", // barna
            "#2b1100", // fekete
        ];
        private readonly static string[] colHair =
        [
            "#ffeeaa", // Szőke
            "#ffcc00", // aranyszőke
            "#ff6600", // vörös
            "#aa4400", // világosbarna
            "#803300", // barna	
            "#552200", //sötétbarna
            "#241c1c", //fekete
            "#E3e2db" // ősz
        ];
        private readonly static string[] colSkin =
        [
            "#f4e3d7", // fal fehér
            "#e9c6af", // nagyon fehér
            "#deaa87", // fehér
            "#d38d5f", // cigány
            "#c87137", // latino		
            "#a05a2c", // arab
            "#784421", // afro-americano
            "#502d16" // nigger
        ];
        private CharacterModel _character = new();
        protected override void OnParametersSet()
        {
            _character = BuildCharacterFromPicCode(PicCode);
        }

        private static CharacterModel BuildCharacterFromPicCode(string piccode)
        {
            // M 1 2 3 4 A G
            // F 1 E3 H4 A G
            // M 8 E4 H0 B N
            //piccode = CharacterGenerator();
            if (piccode == "") return new CharacterModel() { genre = null };
            var skinIndex = Math.Min(int.Parse(piccode[1].ToString()), 7);
            var eyeIndex = Math.Min(int.Parse(piccode[2].ToString()), 3);
            var hairIndex = Math.Min(int.Parse(piccode[3].ToString()), 7);
            var hairstyle = Math.Min(int.Parse(piccode[4].ToString()), 7);
            return new CharacterModel()
            {
                skinColor = colSkin[skinIndex],
                eyeColor = colEye[eyeIndex],
                hairColor = colHair[hairIndex],
                hairStyle = (byte)hairstyle,
                asian = piccode[5] == 'A',
                glasses = piccode[6] == 'G',
                genre = piccode[0] == 'F'
            };
        }

        public void Dispose()
        {

            GC.SuppressFinalize(this);
        }
    }
}
/*
.pic-display {
border: inset 2px #777;
border-radius: 4px;
box-shadow: 0 4px 16px rgba(0,0,0,0.4), inset 0 4px 8px rgba(0,0,0,0.7);
width: 100%;
height: 100%; /* vagy amit szeretnél *
position: relative;
background - color: rgba(210, 255, 57, 0.9);
}

 
 */