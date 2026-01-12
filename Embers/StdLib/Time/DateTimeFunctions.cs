using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Globalization;

namespace Embers.StdLib.Time
{
    [StdLib("now")]
    public class NowFunction : StdFunction
    {
        [Comments("Returns the current date and time.")]
        [Returns(ReturnType = typeof(DateTime))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return DateTime.Now;
        }
    }

    [StdLib("today")]
    public class TodayFunction : StdFunction
    {
        [Comments("Returns the current date with the time set to midnight.")]
        [Returns(ReturnType = typeof(DateTime))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return DateTime.Today;
        }
    }

    [StdLib("parse_date")]
    public class ParseDateTimeFunction : StdFunction
    {
        [Comments("Parses a string into a DateTime object.")]
        [Arguments(ParamNames = new[] { "date_string" }, ParamTypes = new[] { typeof(string) })]
        [Returns(ReturnType = typeof(DateTime))]
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
        [Comments("Formats a DateTime object into a string based on the given format.")]
        [Arguments(ParamNames = new[] { "date_time", "format" }, ParamTypes = new[] { typeof(DateTime), typeof(string) })]
        [Returns(ReturnType = typeof(string))]
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
        [Comments("Returns the year component of a DateTime object.")]
        [Arguments(ParamNames = new[] { "date_time" }, ParamTypes = new[] { typeof(DateTime) })]
        [Returns(ReturnType = typeof(Number))]
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
        [Comments("Returns the month component of a DateTime object.")]
        [Arguments(ParamNames = new[] { "date_time" }, ParamTypes = new[] { typeof(DateTime) })]
        [Returns(ReturnType = typeof(Number))]
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
        [Comments("Returns the day component of a DateTime object.")]
        [Arguments(ParamNames = new[] { "date_time" }, ParamTypes = new[] { typeof(DateTime) })]
        [Returns(ReturnType = typeof(Number))]
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
        [Comments("Returns the hour component of a DateTime object.")]
        [Arguments(ParamNames = new[] { "date_time" }, ParamTypes = new[] { typeof(DateTime) })]
        [Returns(ReturnType = typeof(Number))]
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
        [Comments("Returns the second component of a DateTime object.")]
        [Arguments(ParamNames = new[] { "date_time" }, ParamTypes = new[] { typeof(DateTime) })]
        [Returns(ReturnType = typeof(Number))]
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
