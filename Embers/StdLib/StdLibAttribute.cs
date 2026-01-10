namespace Embers.StdLib
{
    /// <summary>
    /// Attribute to mark a class as a standard library function and specify its registration name.
    /// Can optionally specify target type(s) (e.g., "Array", "String", "Fixnum", "Float") for automatic registration
    /// on specific native classes or language types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class StdLibAttribute : Attribute
    {
        public string[] Names { get; }
        
        /// <summary>
        /// Target type names for automatic registration (e.g., "Array", "String", "Fixnum", "Float").
        /// If null or empty array, the function is registered globally on the root context.
        /// Multiple types can be specified for functions that work on multiple types (e.g., numeric functions).
        /// </summary>
        public string[] TargetTypes { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Legacy single TargetType property for backward compatibility.
        /// Setting this will set TargetTypes to a single-element array.
        /// </summary>
        public string? TargetType 
        { 
            get => TargetTypes.Length > 0 ? TargetTypes[0] : null;
            set => TargetTypes = value != null ? new[] { value } : Array.Empty<string>();
        }

        public StdLibAttribute(params string[] names)
        {
            Names = names ?? Array.Empty<string>();
        }
    }
}
