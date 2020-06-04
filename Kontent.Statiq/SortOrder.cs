namespace Kontent.Statiq
{
    /// <summary>
    /// Sort order of content items retrieved from Kontent.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Sort from low to high.
        /// </summary>
        Ascending = Kentico.Kontent.Delivery.Abstractions.SortOrder.Ascending,
        /// <summary>
        /// Sort from high to low.
        /// </summary>
        Descending = Kentico.Kontent.Delivery.Abstractions.SortOrder.Descending,
    }
}
