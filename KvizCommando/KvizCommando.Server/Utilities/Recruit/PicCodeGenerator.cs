namespace KvizCommando.Server.Utilities.Recruit
{
    public interface IPicCodeGenerator
    {
        string[] GeneratePicCodes(int count);
    }

    public class PicCodeGenerator : IPicCodeGenerator
    {
        public string[] GeneratePicCodes(int count)
        {
            string[] result = new string[count];

            for (int i = 0; i < count; i++)
                result[i] = GenerateOne(i<count/2);
            return result;
        }

        private string GenerateOne(bool sex)
        {
            var rnd = Random.Shared;
            char genre = sex ? 'M' : 'F'; ;
            int skin = rnd.Next(0, 8);
            int eye = skin > 3 ? rnd.Next(2, 4) : rnd.Next(0, 4);
            int hair = (skin > 3 && genre != 'F') ? rnd.Next(4, 8) : rnd.Next(0, 8);
            int hairStyle = genre == 'F' ? rnd.Next(1, 8) : rnd.Next(0, 8);
            char asian = (skin < 4 && rnd.Next(0, 100) < 25) ? 'A' : 'B';
            char glasses = rnd.Next(0, 100) < 25 ? 'G' : 'N';

            return $"{genre}{skin}{eye}{hair}{hairStyle}{asian}{glasses}";
        }
    }
}