using Embers.Exceptions;
using Embers.Language;
using System.Diagnostics;

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
                {
                    var s = Inspect(value);
                    writer.WriteLine(s);
                }

                return (values.Count == 1 ? values[0] : new List<object>(values)).ToString();
            }
            catch (Exception ex)
            {
                writer.WriteLine(ex.Message);
                throw new EmbersError(ex.Message);
            }
        }

        private static string Inspect(object? value)
        {
            if (value == null) return "nil";

            // Strings: quote like Ruby
            if (value is string str)
                return $"\"{Escape(str)}\"";

            // Lists: [a, b]
            if (value is IEnumerable<object> list && value is not string)
                return "[" + string.Join(", ", list.Select(Inspect)) + "]";

            // Dictionaries: {k=>v}
            if (value is IDictionary<object, object> dict)
                return "{" + string.Join(", ", dict.Select(kv => $"{Inspect(kv.Key)}=>{Inspect(kv.Value)}")) + "}";

            // Fallback
            return value.ToString() ?? value.GetType().Name;
        }

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }

}
