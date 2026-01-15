namespace Embers.Compiler;

/// <summary>
/// the character stream implementation that reads from a <see cref="TextReader"/>.
/// </summary>
/// <seealso cref="ICharStream" />
public class TextReaderCharStream(TextReader reader) : ICharStream
{
    private readonly TextReader reader = reader;
    private readonly char[] buffer = new char[1024];
    private int length;
    private int position;

    public int NextChar()
    {
        while (position >= length)
        {
            length = reader.Read(buffer, 0, buffer.Length);

            if (length == 0)
                return -1;

            position = 0;
        }

        return buffer[position++];
    }

    public void BackChar()
    {
        if (position > 0 && position <= length)
            position--;
    }
}

