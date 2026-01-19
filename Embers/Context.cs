using Embers.Functions;

namespace Embers;
/// <summary>
/// Context for the current execution state, including local variables, self object, and block.
/// The context is used to manage the scope of variables and methods during execution.
/// </summary>
public class Context
{
    private readonly Context parent;
    private readonly IDictionary<string, object> values = new Dictionary<string, object>();
    private readonly IDictionary<string, object> globals;
    private DynamicObject self;
    private readonly DynamicClass? module;
    private readonly IFunction? block;

    public IFunction? Block => block;

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    public Context()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="block">The block.</param>
    public Context(Context parent, IFunction? block = null)
    {
        this.parent = parent;
        this.block = block;
        globals = parent == null ? new Dictionary<string, object>() : parent.globals;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="block">The block.</param>
    public Context(DynamicClass module, Context parent, IFunction? block = null)
    {
        this.module = module;
        this.parent = parent;
        this.block = block;
        globals = parent == null ? new Dictionary<string, object>() : parent.globals;
        self = module;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="self">The self.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="block">The block.</param>
    public Context(DynamicObject self, Context parent, IFunction? block = null)
    {
        this.self = self;
        this.parent = parent;
        this.block = block;
        globals = parent == null ? new Dictionary<string, object>() : parent.globals;
    }

    /// <summary>
    /// Gets or sets the self object.
    /// </summary>
    public DynamicObject Self { get { return self; } internal set { self = value; } }

    /// <summary>
    /// Gets the module.
    /// </summary>
    public DynamicClass Module { get { return module; } }

    /// <summary>
    /// Gets the parent context.
    /// </summary>
    public Context Parent { get { return parent; } }

    /// <summary>
    /// Gets the root context.
    /// </summary>
    /// <value>
    /// The root context.
    /// </value>
    public Context RootContext
    {
        get
        {
            if (parent == null)
                return this;

            return parent.RootContext;
        }
    }

    /// <summary>
    /// Sets the local value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public virtual void SetLocalValue(string name, object value) => values[name] = value;

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public void SetValue(string name, object value)
    {
        if (HasLocalValue(name))
        {
            values[name] = value;
        }
        else if (parent != null && parent.HasValue(name))
        {
            parent.SetValue(name, value);
        }
        else
        {
            values[name] = value;
        }
    }

    /// <summary>
    /// Determines whether [has local value] [the specified name].
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>
    ///   <c>true</c> if [has local value] [the specified name]; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool HasLocalValue(string name) => values.ContainsKey(name);

    /// <summary>
    /// Determines whether the specified name has value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>
    ///   <c>true</c> if the specified name has value; otherwise, <c>false</c>.
    /// </returns>
    public bool HasValue(string name)
    {
        if (HasLocalValue(name))
            return true;

        if (parent != null)
            return parent.HasValue(name);

        return false;
    }

    /// <summary>
    /// Gets the local value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public virtual object GetLocalValue(string name) => values[name];

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public object? GetValue(string name)
    {
        if (values.TryGetValue(name, out object? value))
            return value;

        if (parent != null)
            return parent.GetValue(name);

        return null;
    }

    /// <summary>
    /// Gets the local names.
    /// </summary>
    /// <returns></returns>
    public IList<string> GetLocalNames() => [.. values.Keys];

    /// <summary>
    /// Sets a global value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public void SetGlobalValue(string name, object value) => globals[name] = value;

    /// <summary>
    /// Determines whether the specified global name has value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public bool HasGlobalValue(string name) => globals.ContainsKey(name);

    /// <summary>
    /// Gets the global value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public object? GetGlobalValue(string name)
    {
        if (globals.TryGetValue(name, out object? value))
            return value;

        return null;
    }
}

