namespace Embers.Annotations
{     /// <summary>
    /// Attribute to provide top-level comments for an IFunction method.
    /// This will be used for documentation generation
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class CommentsAttribute : Attribute
    {
        public string[] Comments { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsAttribute"/> class.
        /// </summary>
        /// <param name="comments"></param>
        public CommentsAttribute(params string[] comments)
        {
            Comments = comments ?? Array.Empty<string>();
        }
    }
}