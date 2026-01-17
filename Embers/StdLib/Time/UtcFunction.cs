using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Time;

[StdLib("utc", TargetTypes = new[] { "DateTime" })]
public class UtcFunction : StdFunction
{
    [Comments("Returns a UTC DateTime instance for the given date/time.")]
    [Arguments(ParamNames = new[] { "date_time" }, ParamTypes = new[] { typeof(DateTime) })]
    [Returns(ReturnType = typeof(DateTime))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("utc expects a DateTime argument");
        if (values[0] is DateTime dt)
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        throw new TypeError("utc expects a DateTime");
    }
}
