namespace BWin2.Wasm;

internal interface IRandomSource
{
    void Randomize();

    double Next();
}
