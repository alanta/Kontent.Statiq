using Statiq.Common;
using Statiq.Core;
using System;
using System.Collections.Generic;

namespace Kontent.Statiq
{
    /// <summary>
    /// Short-hand module for adding Kontent documents as child documents.
    /// <para>Use the <see cref="AddDocumentsToMetadata"/> with <see cref="KontentConfig"/> helpers for more control.</para>
    /// </summary>
    /// <typeparam name="TParent">The content type containing the </typeparam>
    public sealed class AddKontentDocumentsToMetadata<TParent> : AddDocumentsToMetadata
    {
        /// <summary>
        /// Add Kontent documents as the default children collection.
        /// </summary>
        /// <param name="func">A function that returns the related documents from a page.</param>
        public AddKontentDocumentsToMetadata(Func<TParent, IEnumerable<object>> func)
            : base(Keys.Children, CreateConfig(func))
        {

        }

        /// <summary>
        /// Add Kontent documents as metadata.
        /// </summary>
        /// <param name="key">The metadata key to set</param>
        /// <param name="func">A function that returns the related documents from a page.</param>
        public AddKontentDocumentsToMetadata(string key, Func<TParent, IEnumerable<object>> func ) 
            : base(key, CreateConfig(func))
        {
            
        }

        private static Config<IEnumerable<IDocument>> CreateConfig(Func<TParent, IEnumerable<object>> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            return KontentConfig.GetChildren(func);
        }
    }
}