namespace Embers.Language
{
    /// <summary>
    /// DynamicHash represents a hash table in the Embers language.
    /// </summary>
    public class DynamicHash : Dictionary<object, object>
    {
        public override string ToString()
        {
            var result = "{";

            foreach (var key in Keys)
            {
                var value = this[key];

                if (result.Length > 1)
                    result += ", ";

                result += key.ToString();
                result += "=>";
                result += value.ToString();
            }

            result += "}";

            return result;
        }
    }
}
