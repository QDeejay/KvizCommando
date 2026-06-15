namespace KvizCommando.Server.Utilities
{
    public static class ArrayExtension
    {
        public static int[] AddTo(this int[] a, int[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Arrays must be the same length.");

            int[] result = new int[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] + b[i];
            }
            return result;
        }

    }
}
