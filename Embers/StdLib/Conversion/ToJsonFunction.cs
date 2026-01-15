using Embers.Exceptions;
using Embers.Language;
using Embers.Annotations;
using System.Text.Json;
using System.Collections;

namespace Embers.StdLib.Conversion;

/// <summary>
/// Converts an object to JSON string representation.
/// </summary>
[StdLib("to_json")]
public class ToJsonFunction : StdFunction
{
    [Comments("Converts an object to its JSON string representation.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(string))]
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
            if (target is IDictionary dict)
            {
                var stringDict = new Dictionary<string, object>();
                foreach (DictionaryEntry kvp in dict)
                {
                    stringDict[kvp.Key?.ToString() ?? ""] = ConvertValue(kvp.Value);
                }
                return JsonSerializer.Serialize(stringDict);
            }
            
            // Handle List (Array)
            if (target is IList list)
            {
                var converted = new List<object>();
                foreach (var item in list)
                    converted.Add(ConvertValue(item));
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
        
        if (value is IDictionary dict)
        {
            var stringDict = new Dictionary<string, object>();
            foreach (DictionaryEntry kvp in dict)
            {
                stringDict[kvp.Key?.ToString() ?? ""] = ConvertValue(kvp.Value);
            }
            return stringDict;
        }
        
        if (value is IList list)
        {
            var converted = new List<object>();
            foreach (var item in list)
                converted.Add(ConvertValue(item));
            return converted;
        }
        
        // For DynamicObject or other complex types, return string representation
        if (value is DynamicObject dyn)
        {
            var dynamicDict = new Dictionary<string, object>();
            foreach (var kvp in dyn.GetValues())
            {
                var key = kvp.Key.StartsWith("@", StringComparison.Ordinal)
                    ? kvp.Key[1..]
                    : kvp.Key;
                dynamicDict[key] = ConvertValue(kvp.Value);
            }
            return dynamicDict;
        }

        return value.ToString() ?? "";
    }
}
