namespace Embers.Console;
public class ConsoleReader(TextReader inner) : TextReader
{
    private readonly TextReader _inner = inner;

    public override string ReadLine()
    {
        var line = _inner.ReadLine();
        return line?.Length >= 2 ? line[2..] : string.Empty;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
        }
        base.Dispose(disposing);
    }
}

