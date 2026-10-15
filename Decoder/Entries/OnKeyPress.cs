using Humanizer;

namespace ScratchCompiler.Decoder.Entries;

public class OnKeyPress : CodeEntry
{
    public const string ScratchCode = "event_whenkeypressed";

    private KeyOption _key;
    
    internal OnKeyPress(KeyOption key) => _key = key;
    
    public override string ToString() => $"When {{{_key.Humanize(LetterCasing.Title)}}} is pressed:";
}

public enum KeyOption: byte
{
    Space, Any,
    UpArrow, DownArrow, LeftArrow, RightArrow,
    A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9
}