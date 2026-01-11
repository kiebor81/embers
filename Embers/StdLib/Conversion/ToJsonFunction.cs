using Embers.Exceptions;
using Embers.Language;
using System.Text.Json;

namespace Embers.StdLib.Conversion
{
    /// <summary>
    /// Converts an object to JSON string representation.
    /// </summary>
    [StdLib("to_json")]
    public class ToJsonFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> arguments)
        {
            try
            {
                var target = arguments.Count > 0 ? arguments[0] : self;
                
                // Handle basic types directly
                if (target is string || target is int || target is long || target is double || 
                    target is bool || target is null)
                {
                    return JsonSerializer.Serialize(target);
                }
                
                // Handle Dictionary (Hash) - convert keys to strings for JSON compatibility
                if (target is Dictionary<object, object> dict)
                {
                    var stringDict = new Dictionary<string, object>();
                    foreach (var kvp in dict)
                    {
                        stringDict[kvp.Key.ToString() ?? ""] = ConvertValue(kvp.Value);
                    }
                    return JsonSerializer.Serialize(stringDict);
                }
                
                // Handle List (Array)
                if (target is List<object> list)
                {
                    var converted = list.Select(ConvertValue).ToList();
                    return JsonSerializer.Serialize(converted);
                }

                // For other types, convert to simple representation
                return JsonSerializer.Serialize(ConvertValue(target));
            }
            catch (Exception ex)
            {
                throw new ValueError($"Failed to serialize to JSON: {ex.Message}");
            }
        }

        private object ConvertValue(object value)
        {
            // Convert to JSON-friendly types
            if (value is string || value is int || value is long || value is double || 
                value is bool || value is null)
            {
                return value;
            }
            
            if (value is Dictionary<object, object> dict)
            {
                var stringDict = new Dictionary<string, object>();
                foreach (var kvp in dict)
                {
                    stringDict[kvp.Key.ToString() ?? ""] = ConvertValue(kvp.Value);
                }
                return stringDict;
            }
            
            if (value is List<object> list)
            {
                return list.Select(ConvertValue).ToList();
            }
            
            // For DynamicObject or other complex types, return string representation
            return value.ToString() ?? "";
        }
    }
}
