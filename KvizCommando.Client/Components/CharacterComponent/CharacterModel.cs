namespace KvizCommando.Client.Components.CharacterComponent
{
    internal  class CharacterModel
    {
        
        public bool? genre { get; set; } // false is male
        public bool glasses { get; set; } = false;
        public bool asian { get; set; } = false;
        public string eyeColor { get; set; } = "";
        public string hairColor { get; set; } = "";
        public string skinColor { get; set; } = "";
        public byte hairStyle { get; set; } = 0;
    }
    internal class CharacterParams
    {
        public string ColorEye { get; set; } = "";
        public string ColorHair { get; set; } = "";
        
        public string ColorSkin { get; set; } = "";
        public byte HairStyle { get; set; } = 0;

        public bool Glasses { get; set; } = false;
        public bool asian { get; set; } = false;
    }
}
