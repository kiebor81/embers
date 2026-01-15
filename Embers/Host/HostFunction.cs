using Embers.Annotations;
using Embers.Functions;
using Embers.Language;

namespace Embers.Host;
/// <summary>
/// Host function base class for all functions that can be registered in the host environment.
/// This class provides a common interface for applying functions in the host environment for building custom DSLs.
/// </summary>
[ScannerIgnore]
public abstract class HostFunction : IFunction
{
    public abstract object Apply(DynamicObject self, Context context, IList<object> values);
}

