using Embers.Language;

namespace Embers.StdLib
{
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
    /// Our global representation for debug and inspection: `p` in Ruby
    /// </summary>
    public class PFunction(TextWriter writer) : StdFunction
    {
        private readonly TextWriter writer = writer;

        public PFunction() : this(Console.Out) { }

        [Annotations.Comments("Prints the given input's raw internal representation to the standard output.")]
        [Annotations.Arguments(ParamNames = new[] { "input" }, ParamTypes = new[] { typeof(object) })]
        [Annotations.Returns(ReturnType = typeof(void))]
        public override object? Apply(DynamicObject self, Context context, IList<object> values)
        {
            foreach (var value in values)
                writer.WriteLine(value?.ToString() ?? "nil");

            return null;
        }
    }
}
