
using System.Text.Json;

namespace KvizCommando.Server.Utilities
{
    public static class ArrayExtension
    {
        /// <summary>
        /// Elemenként összead két azonos hosszúságú egész tömböt.
        /// </summary>
        /// <param name="a">Az első összefűzendő tömb.</param>
        /// <param name="b">A második összefűzendő tömb.</param>
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

        /// <summary>JSON-szövegből tömböt készít.</summary>
        /// <typeparam name="T">A tömb elemeinek típusa.</typeparam>
        /// <param name="json">A feldolgozandó JSON-szöveg.</param>
        /// <returns>A deszerializált tömb; üres vagy <see langword="null"/> bemenetnél üres tömb.</returns>
        public static T[] ConvertToArray<T>(this string? json)
        {
            return string.IsNullOrEmpty(json)
                ? []
                : JsonSerializer.Deserialize<T[]>(json) ?? [];
        }
    }
}
