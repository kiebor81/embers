namespace Embers.Annotations;
/// <summary>
/// Attribute to provide top-level comments for an IFunction method.
/// This will be used for documentation generation
/// </summary>
/// <seealso cref="System.Attribute" />
/// <remarks>
/// Initializes a new instance of the <see cref="CommentsAttribute"/> class.
/// </remarks>
/// <param name="comments"></param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class CommentsAttribute(params string[] comments) : Attribute
{
    public string[] Comments { get; } = comments ?? [];
}
