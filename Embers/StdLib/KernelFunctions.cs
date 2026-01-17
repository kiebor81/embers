using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib;

[StdLib("sleep", "pause")]
public class SleepFunction : StdFunction
{
    [Comments("Pauses execution for the given number of seconds.")]
    [Arguments(ParamNames = new[] { "seconds" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(void))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values != null && values.Count > 0 && values[0] != null)
        {
            double seconds;
            if (values[0] is int i)
                seconds = i;
            else if (values[0] is long l)
                seconds = l;
            else if (values[0] is double d)
                seconds = d;
            else
                throw new TypeError("sleep expects a numeric argument");

            if (seconds > 0)
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        return null;
    }
}

[StdLib("gets")]
public class GetsFunction : StdFunction
{
    [Comments("Reads a line from standard input.")]
    [Returns(ReturnType = typeof(string))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        return Console.In.ReadLine();
    }
}

[StdLib("exit", "bye!")]
public class ExitFunction : StdFunction
{
    [Comments("Stops script execution by raising an EmbersError.")]
    [Arguments(ParamNames = new[] { "status" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(void))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var status = (values != null && values.Count > 0) ? values[0] : null;
        var message = status == null ? "exit" : $"exit {status}";
        throw new EmbersError(message);
    }
}
