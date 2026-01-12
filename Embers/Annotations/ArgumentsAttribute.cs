namespace Embers.Annotations
{

    /// <summary>
    /// Attribute to specify parameter names and types for an IFunction method.
    /// This will be used for documentation generation
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ArgumentsAttribute : Attribute
    {
        /// <summary>
        /// Parameter names and types for the annotated method.
        /// </summary>
        public required string[] ParamNames { get; set; }
        /// <summary>
        /// Parameter types for the annotated method.
        /// </summary>
        public required Type[] ParamTypes { get; set; }

    }
}