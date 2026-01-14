using System.Collections;
using Embers.Functions;
using Embers.Language;

namespace Embers.Utilities;
/// <summary>
/// ObjectUtilities provides methods for manipulating objects, including getting and setting values,
/// It is used to access properties and fields dynamically, handle indexed values,bind event handlers, and check types.
/// Objectutilities is essential for dynamic object manipulation in Embers, and adds the bridge between Embers and .NET types, allowing for dynamic access to properties, methods, and events.
/// </summary>
public class ObjectUtilities
{
    public static void SetValue(object obj, string name, object value)
    {
        Type type = obj.GetType();

        type.InvokeMember(name, System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, obj, [value]);
    }

    public static object GetValue(object obj, string name)
    {
        Type type = obj.GetType();

        try
        {
            return type.InvokeMember(name, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | /* System.Reflection.BindingFlags.InvokeMethod | */ System.Reflection.BindingFlags.Instance, null, obj, null);
        }
        catch
        {
            //return type.GetMethod(name);

            // New: Try custom GetMethod(string) on the object itself
            var getMethod = type.GetMethod("GetMethod", [typeof(string)]);
            if (getMethod != null)
            {
                var method = getMethod.Invoke(obj, [name]);
                if (method != null)
                    return method;
            }

            return null;
        }
    }

    public static object GetValue(object obj, string name, IList<object> arguments) => GetNativeValue(obj, name, arguments);

    public static IList<string> GetNames(object obj) => TypeUtilities.GetNames(obj.GetType());

    public static object GetNativeValue(object obj, string name, IList<object> arguments)
    {
        Type type = obj.GetType();

        // Convert long arguments to int for .NET interop
        // .NET methods typically expect int, but Embers now uses long as default integer type
        object[]? args = null;
        if (arguments != null)
        {
            args = new object[arguments.Count];
            for (int i = 0; i < arguments.Count; i++)
            {
                if (arguments[i] is long longValue && longValue >= int.MinValue && longValue <= int.MaxValue)
                    args[i] = (int)longValue;
                else
                    args[i] = arguments[i];
            }
        }

        try
        {
            return type.InvokeMember(name, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance, null, obj, args);
        }
        catch
        {
            var getMethod = type.GetMethod("GetMethod", [typeof(string)]);
            if (getMethod != null)
            {
                var method = getMethod.Invoke(obj, [name]);
                if (method != null)
                    return method;
            }

            return null;
        }

    }

    public static bool IsNumber(object obj) => obj is long ||
            obj is int ||
            obj is short ||
            obj is decimal ||
            obj is double ||
            obj is float ||
            obj is byte;

    // TODO implement a method with only one index
    public static object GetIndexedValue(object obj, object[] indexes)
    {
        if (obj is Array array)
            return GetIndexedValue(array, indexes);

        if (obj is IList list)
            return GetIndexedValue(list, indexes);

        if (obj is IDictionary dictionary)
            return GetIndexedValue(dictionary, indexes);

        if (obj is DynamicObject @object && indexes != null && indexes.Length == 1)
            return @object.GetValue((string)indexes[0]);

        return GetValue(obj, string.Empty, indexes);
    }

    // TODO implement a method with only one index
    public static void SetIndexedValue(object obj, object[] indexes, object value)
    {
        if (obj is Array array)
        {
            SetIndexedValue(array, indexes, value);
            return;
        }

        if (obj is IList list1)
        {
            if (indexes.Length != 1)
                throw new InvalidOperationException("Invalid number of subindices");

            int index = (int)indexes[0];

            IList list = list1;

            if (list.Count == index)
                list.Add(value);
            else
                list[index] = value;

            return;
        }

        if (obj is IDictionary dictionary)
        {
            if (indexes.Length != 1)
                throw new InvalidOperationException("Invalid number of subindices");

            dictionary[indexes[0]] = value;

            return;
        }

        // TODO as in GetIndexedValue, consider Default member
        throw new InvalidOperationException(string.Format("Not indexed value of type {0}", obj.GetType().ToString()));
    }

    public static void SetIndexedValue(Array array, object[] indexes, object value)
    {
        switch (indexes.Length)
        {
            case 1:
                array.SetValue(value, (int)indexes[0]);
                return;
            case 2:
                array.SetValue(value, (int)indexes[0], (int)indexes[1]);
                return;
            case 3:
                array.SetValue(value, (int)indexes[0], (int)indexes[1], (int)indexes[2]);
                return;
        }

        throw new InvalidOperationException("Invalid number of subindices");
    }

    public static void AddHandler(object obj, string eventname, IFunction function, Context context)
    {
        var type = obj.GetType();
        var @event = type.GetEvent(eventname);
        var invoke = @event.EventHandlerType.GetMethod("Invoke");
        var parameters = invoke.GetParameters();
        int npars = parameters.Count();
        _ = new Type[npars + 1];
        Type wrappertype = null;
        Type[] partypes = new Type[npars + 2];
        Type rettype = invoke.ReturnParameter.ParameterType;
        bool isaction = rettype.FullName == "System.Void";

        if (isaction)
            rettype = typeof(int);

        switch (npars)
        {
            case 0:
                partypes[0] = rettype;
                partypes[1] = @event.EventHandlerType;
                wrappertype = typeof(FunctionWrapper<,>).MakeGenericType(partypes);
                break;
            case 1:
                partypes[0] = parameters.ElementAt(0).ParameterType;
                partypes[1] = rettype;
                partypes[2] = @event.EventHandlerType;
                wrappertype = typeof(FunctionWrapper<,,>).MakeGenericType(partypes);
                break;
            case 2:
                partypes[0] = parameters.ElementAt(0).ParameterType;
                partypes[1] = parameters.ElementAt(1).ParameterType;
                partypes[2] = rettype;
                partypes[3] = @event.EventHandlerType;
                wrappertype = typeof(FunctionWrapper<,,,>).MakeGenericType(partypes);
                break;
            case 3:
                partypes[0] = parameters.ElementAt(0).ParameterType;
                partypes[1] = parameters.ElementAt(1).ParameterType;
                partypes[2] = parameters.ElementAt(2).ParameterType;
                partypes[3] = rettype;
                partypes[4] = @event.EventHandlerType;
                wrappertype = typeof(FunctionWrapper<,,,,>).MakeGenericType(partypes);
                break;
        }

        object wrapper = Activator.CreateInstance(wrappertype, function, context);

        @event.AddEventHandler(obj, (Delegate)GetValue(wrapper, isaction ? "CreateActionDelegate" : "CreateFunctionDelegate", null));
    }

    private static object GetIndexedValue(Array array, object[] indexes)
    {
        switch (indexes.Length)
        {
            case 1:
                return array.GetValue((int)indexes[0]);
            case 2:
                return array.GetValue((int)indexes[0], (int)indexes[1]);
            case 3:
                return array.GetValue((int)indexes[0], (int)indexes[1], (int)indexes[2]);
            default:
                break;
        }

        throw new InvalidOperationException("Invalid number of subindices");
    }

    private static object GetIndexedValue(IList list, object[] indexes)
    {
        if (indexes.Length != 1)
            throw new InvalidOperationException("Invalid number of subindices");

        return list[(int)indexes[0]];
    }

    private static object GetIndexedValue(IDictionary dictionary, object[] indexes)
    {
        if (indexes.Length != 1)
            throw new InvalidOperationException("Invalid number of subindices");

        return dictionary[indexes[0]];
    }
}

