namespace Orchard.Taxonomies.Settings {
    public class TermPartSettings {
        /// <summary>
        /// The display type to use for the child items of the term.
        /// </summary>
        public string ChildDisplayType { get; set; }

        /// <summary>
        /// If true, overrides default pagination settings with the PageSize value.
        /// </summary>
        public bool OverrideDefaultPagination { get; set; }

        /// <summary>
        /// The page size, if OverrideDefaultPagination is set to true.
        /// </summary>
        public int PageSize { get; set; }
    }
}