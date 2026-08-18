namespace KvizCommando.Client.Helpers
{
    public static class DateTimeHelpers
    {
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
