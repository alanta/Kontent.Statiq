// This code was generated by a kontent-generators-net tool 
// (see https://github.com/Kentico/kontent-generators-net).
// 
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated. 
// For further modifications of the class, create a separate file with the partial class.

using Kentico.Kontent.Delivery.Abstractions;
using System.Collections.Generic;

namespace Kontent.Statiq.Tests.Models
{
    public partial class FactAboutUs
    {
        public const string Codename = "fact_about_us";
        public const string DescriptionCodename = "description";
        public const string ImageCodename = "image";
        public const string SitemapCodename = "sitemap";
        public const string TitleCodename = "title";

        public IRichTextContent Description { get; set; }
        public IEnumerable<IAsset> Image { get; set; }
        public IEnumerable<ITaxonomyTerm> Sitemap { get; set; }
        public IContentItemSystemAttributes System { get; set; }
        public string Title { get; set; }
    }
}