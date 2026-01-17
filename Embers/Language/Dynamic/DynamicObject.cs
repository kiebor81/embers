using Embers.Functions;

namespace Embers.Language.Dynamic;
/// <summary>
/// DynamicObject represents an instance of a DynamicClass in the Embers language.  
/// </summary>
public class DynamicObject(DynamicClass @class)
{
    private DynamicClass @class = @class;
    private DynamicClass singletonclass;
    private readonly IDictionary<string, object> values = new Dictionary<string, object>();

    /// <summary>
    /// Gets the class of this object.
    /// </summary>
    public DynamicClass @Class { get { return @class; } }

    /// <summary>
    /// Gets the singleton class for this object.
    /// </summary>
    public DynamicClass SingletonClass
    {
        get
        {
            if (singletonclass == null)
            {
                singletonclass = new DynamicClass(string.Format("#<Class:{0}>", ToString()), @class);
                singletonclass.SetClass(@class.Class);
            }

            return singletonclass;
        }
    }

    /// <summary>
    /// Sets a value for this object.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetValue(string name, object value) => values[name] = value;

    /// <summary>
    /// Gets a value by name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object? GetValue(string name)
    {
        if (values.TryGetValue(name, out object? value))
            return value;

        return null;
    }

    /// <summary>
    /// Gets all values of this object.
    /// Useful for reflection purposes and serialization.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<string, object> GetValues() => new Dictionary<string, object>(values);

    /// <summary>
    /// Gets a method for this object by name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual IFunction? GetMethod(string name)
    {
        if (singletonclass != null)
            return singletonclass.GetInstanceMethod(name);

        if (@class != null)
            return @class.GetInstanceMethod(name);

        return null;
    }

    public override string ToString() => string.Format("#<{0}:0x{1}>", Class.Name, GetHashCode().ToString("x"));

    /// <summary>
    /// Sets the class of this object.
    /// </summary>
    /// <param name="class"></param>
    internal void SetClass(DynamicClass @class) => this.@class = @class;
}

