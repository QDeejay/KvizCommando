namespace KvizCommando.Client.Helpers
{
    public static class DictionaryExtensions
    {
        /// <summary>A forrás bejegyzéseit hozzáadja a célszótárhoz, az azonos kulcsú értékeket felülírva.</summary>
        /// <typeparam name="TKey">A kulcs típusa.</typeparam>
        /// <typeparam name="TValue">Az érték típusa.</typeparam>
        /// <param name="target">A módosítandó célszótár.</param>
        /// <param name="source">A bemásolandó bejegyzések.</param>
        public static void AddRange<TKey, TValue>(
            this IDictionary<TKey, TValue> target,
            IDictionary<TKey, TValue> source)
        {
            foreach (var kv in source)
                target[kv.Key] = kv.Value;
        }
    }
}
