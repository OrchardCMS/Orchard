namespace Orchard.UI.PageTitle {
    public interface IPageTitleBuilder : IDependency {

        /// <summary>
        /// Adds some strings at the end of the title.
        /// </summary>
        /// <param name="titleParts">A set of strings to add at the end of the title.</param>
        void AddTitleParts(params string[] titleParts);
        
        /// <summary>
        /// Inserts some strings at the beginning of the title.
        /// </summary>
        /// <param name="titleParts">A set of strings to insert at the beginning of the title.</param>
        void AppendTitleParts(params string[] titleParts);
        
        /// <summary>
        /// Concatenates every title parts using the separator defined in settings.
        /// </summary>
        /// <returns>A string representing the aggregate title for the current page.</returns>
        string GenerateTitle();
    }
}