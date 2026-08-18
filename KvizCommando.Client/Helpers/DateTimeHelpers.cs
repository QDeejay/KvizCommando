namespace KvizCommando.Client.Helpers
{
    public static class DateTimeHelpers
    {
        /// <summary>
        /// Kiszámítja a célidőpontig hátralévő teljes órákat és perceket.
        /// </summary>
        /// <param name="utcTarget">A visszaszámlálás céljának UTC időpontja.</param>
        /// <param name="hours">A kiszámított teljes órák kimeneti értéke.</param>
        /// <param name="minutes">A fennmaradó percek kimeneti értéke.</param>
        public static void GetTimeLeft(DateTime utcTarget, out int hours, out int minutes)
        {
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan diff = (utcTarget - utcNow) + TimeSpan.FromMinutes(1);

            if (diff.TotalSeconds <= 0)
            {
                hours = 0;
                minutes = 0;
                return;
            }

            hours = (int)diff.TotalHours;
            minutes = diff.Minutes;
        }
    }
}
