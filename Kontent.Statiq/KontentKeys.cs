namespace Kontent.Statiq
{
    /// <summary>
    /// Keys for well-known Statiq document metadata for Kontent.
    /// </summary>
    public static class KontentKeys
    {
        /// <summary>
        /// The key used to store the Kontent item in Statiq document metadata.
        /// </summary>
        public const string Item = "KONTENT";
        
        /// <summary>
        /// Keys for well-known Statiq document metadata for Kontent system metadata.
        /// </summary>
        public static class System
        {
            /// <summary>
            /// The item name.
            /// </summary>
            public const string Name = "system.name";
            /// <summary>
            /// The item code name.
            /// </summary>
            public const string CodeName = "system.codename";
            /// <summary>
            /// The language for the item.
            /// </summary>
            public const string Language = "system.language";
            /// <summary>
            /// The unique id for the item.
            /// </summary>
            public const string Id = "system.id";
            /// <summary>
            /// The type code name of the content.
            /// </summary>
            public const string Type = "system.type";
            /// <summary>
            /// The last modified date. This is a <c>DateTime</c> value.
            /// </summary>
            public const string LastModified = "system.lastmodified";
        }

        /// <summary>
        /// Keys for well-known Statiq document metadata for image processing.
        /// </summary>
        public static class Images
        {
            /// <summary>
            /// The key used by the <see cref="KontentImageProcessor"/> and <see cref="KontentDownloadImages"/> modules to handle
            /// images that need to be downloaded.
            /// </summary>
            public const string Downloads = "KONTENT-ASSET-DOWNLOADS";
        }

        public static class Taxonomy
        {
            /// <summary>
            /// The key used to store the Kontent taxonomy group in Statiq document metadata.
            /// </summary>
            public const string Group = "KONTENT-TAXONOMY-GROUP";
            /// <summary>
            /// The key used to store the Kontent taxonomy terms in Statiq document metadata.
            /// </summary>
            public const string Terms = "KONTENT-TAXONOMY-TERMS";
            /// <summary>
            /// The key used to store the Kontent taxonomy term in Statiq document metadata.
            /// </summary>
            public const string Term = "KONTENT-TAXONOMY-TERM";
        }
    }
}