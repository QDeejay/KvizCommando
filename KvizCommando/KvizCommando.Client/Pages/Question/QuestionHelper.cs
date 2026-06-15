using KvizCommando.Client.Services.Visual;

namespace KvizCommando.Client.Pages.Question
{
    public sealed class QuestionHelper
    {
        internal static bool ArraysEqual(int[] a, int[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
        internal static string GetLabel(CategoryOption opt, string culture)
        {
            return culture switch
            {
                "hu" => opt.LabelHu,
                "en" => opt.LabelEn,
                _ => opt.LabelHu
            };
        }
        internal static (int[] original, int[] working) CloneFactorySlots(int[] factSlots)
        {
            int length = factSlots.Length;
            int[] original = new int[length];
            int[] working = new int[length];
            for (var i = 0; i < length; i++)
            {
                original[i] = factSlots[i];
                working[i] = original[i];
            }
            return (original, working);
        }
    }
}
