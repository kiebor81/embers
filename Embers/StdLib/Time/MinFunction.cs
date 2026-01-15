using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Time;

[StdLib("min", TargetTypes = new[] { "DateTime" })]
public class MinFunction : StdFunction
{
    [Comments("Returns the minute component of a DateTime object.")]
    [Arguments(ParamNames = new[] { "date_time" }, ParamTypes = new[] { typeof(DateTime) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("min expects a DateTime argument");
        if (values[0] is DateTime dt)
            return dt.Minute;
        throw new TypeError("min expects a DateTime");
    }
}
