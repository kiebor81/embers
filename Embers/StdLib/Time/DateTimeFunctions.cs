using Embers.Language;
using Embers.Exceptions;
using System.Globalization;

namespace Embers.StdLib.Time
{
    [StdLib("now")]
    public class NowFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return DateTime.Now;
        }
    }

    [StdLib("today")]
    public class TodayFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return DateTime.Today;
        }
    }

    [StdLib("parse_date")]
    public class ParseDateTimeFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("parse expects a string argument");
            if (values[0] is string s)
                return DateTime.Parse(s, CultureInfo.InvariantCulture);
            throw new TypeError("parse expects a string");
        }
    }

    [StdLib("strftime", TargetTypes = new[] { "DateTime" })]
    public class StrftimeFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("strftime expects a DateTime and a format string");
            if (values[0] is DateTime dt && values[1] is string fmt)
                return dt.ToString(fmt, CultureInfo.InvariantCulture);
            throw new TypeError("strftime expects a DateTime and a format string");
        }
    }

    [StdLib("year", TargetTypes = new[] { "DateTime" })]
    public class YearFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("year expects a DateTime argument");
            if (values[0] is DateTime dt)
                return dt.Year;
            throw new TypeError("year expects a DateTime");
        }
    }

    [StdLib("month", TargetTypes = new[] { "DateTime" })]
    public class MonthFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("month expects a DateTime argument");
            if (values[0] is DateTime dt)
                return dt.Month;
            throw new TypeError("month expects a DateTime");
        }
    }

    [StdLib("day", TargetTypes = new[] { "DateTime" })]
    public class DayFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("day expects a DateTime argument");
            if (values[0] is DateTime dt)
                return dt.Day;
            throw new TypeError("day expects a DateTime");
        }
    }

    [StdLib("hour", TargetTypes = new[] { "DateTime" })]
    public class HourFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("hour expects a DateTime argument");
            if (values[0] is DateTime dt)
                return dt.Hour;
            throw new TypeError("hour expects a DateTime");
        }
    }

    [StdLib("sec", TargetTypes = new[] { "DateTime" })]
    public class SecFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("sec expects a DateTime argument");
            if (values[0] is DateTime dt)
                return dt.Second;
            throw new TypeError("sec expects a DateTime");
        }
    }
}
