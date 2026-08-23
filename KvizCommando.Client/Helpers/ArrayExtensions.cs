namespace KvizCommando.Client.Helpers
{
    public static class ArrayExtensions
    {
        private static readonly Random rng = new();

        /// <summary>Véletlenszerű sorrendbe rendezi a tömb elemeit.</summary>
        /// <typeparam name="T">A tömb elemeinek típusa.</typeparam>
        /// <param name="array">A helyben keverendő tömb.</param>
        public static void Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (array[k], array[n]) = (array[n], array[k]);
            }
        }
    }
}
