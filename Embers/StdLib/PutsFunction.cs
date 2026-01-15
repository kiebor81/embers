using Embers.Exceptions;
using Embers.Language;

namespace Embers.StdLib;

[StdLib("puts", "print")]
/// <summary>
/// Our global representation for Console.WriteLine: `puts` in Ruby
/// </summary>
public class PutsFunction(TextWriter writer) : StdFunction
{
    private readonly TextWriter writer = writer;

    public PutsFunction() : this(Console.Out) { }

    [Annotations.Comments("Prints the given input to the standard output with a newline.")]
    [Annotations.Arguments(ParamNames = new[] { "input" }, ParamTypes = new[] { typeof(string) })]
    [Annotations.Returns(ReturnType = typeof(void))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        foreach (var value in values)
            writer.WriteLine(value);

        return null;
    }
}

[StdLib("p")]
/// <summary>
/// Ruby-like debug print: prints inspect-style representation and returns the argument(s).
/// </summary>
public class PFunction(TextWriter writer) : StdFunction
{
    private readonly TextWriter writer = writer;

    public PFunction() : this(Console.Out) { }

    [Annotations.Comments("Prints the given input's inspect-style representation to standard output and returns the argument(s).")]
    [Annotations.Arguments(ParamNames = new[] { "values" }, ParamTypes = new[] { typeof(object[]) })]
    [Annotations.Returns(ReturnType = typeof(object))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        try
        {
            if (values == null || values.Count == 0)
                return null;

            foreach (var value in values)
                writer.WriteLine(InspectUtils.Inspect(value));

            return (values.Count == 1 ? values[0] : new List<object>(values)).ToString();
        }
        catch (Exception ex)
        {
            writer.WriteLine(ex.Message);
            throw new EmbersError(ex.Message);
        }
    }

    
}

