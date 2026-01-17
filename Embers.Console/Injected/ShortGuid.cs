using Embers.Host;
using Embers.Annotations;
using Embers.Language.Dynamic;

namespace Embers.Console.Injected
{
    public static class ShortGuid
    {
        public static string NewShortGuid()
        {
            var guid = Guid.NewGuid();
            return Convert.ToBase64String(guid.ToByteArray())
                .Replace("/", "_").Replace("+", "-")[..22];
        }
    }

    [HostFunction("short_guid")]
    internal class ShortGuidFunction : HostFunction
    {
        [Comments("Generates a new short GUID.")]
        [Returns(ReturnType = typeof(string))]
        public override object Apply(DynamicObject self, Context context, IList<object> values) => ShortGuid.NewShortGuid();
    }
}
