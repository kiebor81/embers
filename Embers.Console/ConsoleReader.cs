namespace Embers.Console;
public class ConsoleReader(TextReader inner) : TextReader
{
    private readonly TextReader _inner = inner;

    public override string ReadLine()
    {
        var line = _inner.ReadLine();
        return line?.Length >= 2 ? line[2..] : string.Empty;
    }

    // Optional: override other members as needed, depending on your parser
}

