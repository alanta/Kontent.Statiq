using System.Collections.Generic;

namespace Kontent.Statiq.Metadata
{
    public static class DefaultElementParser
    {
        public static bool TryParseMetadata(dynamic element, out KeyValuePair<string, object> metadata )
        {
            metadata = new KeyValuePair<string, object>(element.Name, element.Value.value);
            return true;
        }
    }
}