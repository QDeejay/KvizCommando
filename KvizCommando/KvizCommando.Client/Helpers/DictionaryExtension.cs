namespace KvizCommando.Client.Helpers
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(
            this IDictionary<TKey, TValue> target,
            IDictionary<TKey, TValue> source)
        {
            foreach (var kv in source)
                target[kv.Key] = kv.Value;
        }
    }
}
