using Kentico.Kontent.Delivery.Abstractions;

namespace Kontent.Statiq
{
    // This is used to map ITaxonomyTermDetails to ITaxonomyTerm and make it easier to 
    // compare and group content by taxonomy.
    internal class TaxonomyTerm : ITaxonomyTerm
    {
        private TaxonomyTerm(string codename, string name)
        {
            Codename = codename;
            Name = name;
        }

        public string Codename { get; }
        public string Name { get; }

        public static ITaxonomyTerm CreateFrom(ITaxonomyTermDetails details)
        {
            return new TaxonomyTerm(details.Codename, details.Name);
        }
    }
}