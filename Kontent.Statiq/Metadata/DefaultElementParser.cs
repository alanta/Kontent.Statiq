using System.Collections.Generic;

namespace Kontent.Statiq.Metadata
{
    /// <summary>
    /// The default element parser. Used to convert elements in the Kontent object model to metadata for a Statiq document.
    /// </summary>
    public static class DefaultElementParser
    {
        /// <summary>
        /// Try to convert a Kontent element into Statiq metadata.
        /// </summary>
        /// <param name="element">The Kontent item element.</param>
        /// <param name="metadata">The metadata created from the element.</param>
        /// <returns><c>true</c></returns>
        public static bool TryParseMetadata(dynamic element, out KeyValuePair<string, object> metadata )
        {
            metadata = new KeyValuePair<string, object>(element.Name, element.Value.value);
            return true;
        }
    }
}