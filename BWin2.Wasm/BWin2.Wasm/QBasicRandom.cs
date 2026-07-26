namespace BWin2.Wasm;

/// <summary>
/// 24-bit linear generator used by classic Microsoft BASIC RND.
/// The Timer seed is intentionally refreshed at the same two places as BUILD4.
/// </summary>
internal sealed class QBasicRandom : IRandomSource
{
    private const int Mask = 0x00FF_FFFF;
    private int _seed;

    public QBasicRandom() => Randomize();

    public void Randomize()
    {
        DateTime now = DateTime.Now;
        double seconds =
            now.Hour * 3600 +
            now.Minute * 60 +
            now.Second +
            now.Millisecond / 1000d;

        _seed = ((int)(seconds * 1000) ^ Environment.TickCount) & Mask;
        if (_seed == 0)
            _seed = 0x50000;
    }

    public double Next()
    {
        _seed = (int)(((long)_seed * 1_140_671_485L + 12_820_163L) & Mask);
        return _seed / 16_777_216d;
    }
}
