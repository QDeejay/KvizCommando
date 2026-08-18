using KvizCommando.Client.Services.Visual;

namespace KvizCommando.Client.Helpers
{
    public class CategoryOptionHelpers
    {
        private readonly ICategoryLookupService _lookup;
        public CategoryOptionHelpers(ICategoryLookupService lookup)
        {
            _lookup = lookup;
        }
        public CategoryOption[] OptionsUpdate(optionType otype, bool[] categorymask)
        {
            var MergedMask = new bool[18];


            MergedMask[0] = otype == optionType.Normal || otype == optionType.Own;
            Array.Copy(categorymask, 0, MergedMask, 1, 8);
            Array.Copy(categorymask, 0, MergedMask, 9, 8);
            MergedMask[17] = otype == optionType.Own;

            var indicates = ToTrueIndicates(MergedMask);
            var all = _lookup.GetAll();
            var index = 0;
            var OptionWinLenght = indicates.Length;
            var catOption = new CategoryOption[OptionWinLenght];
            for (var i = 0; i < OptionWinLenght; i++)
            {
                index = indicates[i];
                catOption[i] = all[index];
            }
            return catOption;
        }
        public string ResolveLabel(int code, string culture)
        {
            return _lookup.ResolveLabel(code, culture);
        }
        public enum optionType
        {
            Normal,
            Own,
            New,
        }
        private static int[] ToTrueIndicates(bool[] source)
        {
            int count = 0;
            for (int i = 0; i < source.Length; i++)
                if (source[i]) count++;

            int pos = 0;
            int[] indices = new int[count];
            for (int i = 0; i < source.Length; i++)
                if (source[i])
                    indices[pos++] = i;

            return indices;
        }
    }
}
