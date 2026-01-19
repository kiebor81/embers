using System.Text.RegularExpressions;

namespace Embers.Language.Primitive;

/// <summary>
/// Regexp represents a compiled regular expression.
/// </summary>
public sealed class Regexp
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(100);

    public Regexp(string pattern, RegexOptions options = RegexOptions.None)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        Options = options;
        Regex = new Regex(pattern, options, DefaultTimeout);
    }

    public string Pattern { get; }

    public RegexOptions Options { get; }

    public Regex Regex { get; }

    public override string ToString() => $"/{Pattern}/";
}
