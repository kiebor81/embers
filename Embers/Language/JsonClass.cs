using System.Text.Json;
using Embers.Functions;

namespace Embers.Language;
/// <summary>
/// JsonClass provides JSON parsing and serialization capabilities.
/// </summary>
public class JsonClass(Machine machine) : NativeClass("JSON", machine)
{
    public override IFunction? GetMethod(string name)
    {
        if (name == "parse")
        {
            return new LambdaFunction(ParseLambda);
        }
        return base.GetMethod(name);
    }

    private static object ParseLambda(DynamicObject self, Context context, IList<object> arguments)
    {
        if (arguments.Count == 0)
            throw new Exceptions.ArgumentError("JSON.parse requires at least 1 argument (json string)");

        var jsonString = arguments[0]?.ToString() ?? "";

        // If a type is provided as second argument, deserialize to that type
        if (arguments.Count > 1 && arguments[1] is Type targetType)
        {
            return JsonSerializer.Deserialize(jsonString, targetType) ?? new object();
        }

        // Default: deserialize to Dictionary (Hash)
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Try to deserialize as object (which will become Dictionary)
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString, options);

        return JsonElementToObject(jsonElement);
    }

    private static object JsonElementToObject(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<object, object>();
                foreach (var property in element.EnumerateObject())
                {
                    dict[property.Name] = JsonElementToObject(property.Value);
                }
                return dict;

            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(JsonElementToObject(item));
                }
                return list;

            case JsonValueKind.String:
                return element.GetString() ?? "";

            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intValue))
                    return intValue;
                if (element.TryGetInt64(out long longValue))
                    return longValue;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null!;

            default:
                return element.ToString();
        }
    }
}

