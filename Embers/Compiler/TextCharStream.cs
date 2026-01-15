namespace Embers.Compiler;

/// <summary>
/// text-based implementation of <see cref="ICharStream"/>.
/// </summary>
/// <seealso cref="ICharStream" />
public class TextCharStream(string text) : ICharStream
{
    private readonly string text = text;
    private int position = 0;

    public int NextChar()
    {
        if (position >= text.Length)
            return -1;

        return text[position++];
    }

    public void BackChar()
    {
        if (position > 0 && position <= text.Length)
            position--;
    }
}

