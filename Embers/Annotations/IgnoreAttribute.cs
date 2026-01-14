namespace Embers.Annotations;
/// <summary>
/// Indicates that the marked element should be ignored by certain processes, such as serialization or mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ScannerIgnoreAttribute : Attribute { }
