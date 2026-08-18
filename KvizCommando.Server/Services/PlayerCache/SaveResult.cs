using System.Numerics;

public enum SaveResult
{
    None = 0,   // nincs mit menteni
    Dirty = 1,  // volt dirty és elmentve
    Logout = 2,  // logout flag miatt cache-ből eltávolítva
    Obscolated = 3, // lejárt
}

public class QuestionStats
{
    public int totalQuestions { get; set; } = 0;
    public int userQuestions { get; set; } = 0;
    public int pendingQuestions { get; set; } = 0;
    public int transferedQuestions { get; set; } = 0;
}