namespace Embers.Annotations
{     /// <summary>
    /// Attribute to specify return type for an IFunction method.
    /// This will be used for documentation generation
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ReturnsAttribute : Attribute
    {
        /// <summary>
        /// Return type for the annotated method.
        /// </summary>
        public required Type ReturnType { get; set; }
    }
}