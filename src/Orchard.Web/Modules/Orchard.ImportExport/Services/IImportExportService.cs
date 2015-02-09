using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        /// <summary>
        /// Import a recipe or package.
        /// </summary>
        /// <param name="recipeOrPackage">The stream for the recipe or package.</param>
        /// <param name="fileName">The file name for the recipe or package.</param>
        /// <returns>The execution id for the import operation.</returns>
        string Import(Stream recipeOrPackage, string fileName);
        /// <summary>
        /// Import a recipe.
        /// </summary>
        /// <param name="recipeText">The XML string of the recipe.</param>
        /// <param name="filesPath">An optional path to a directory that contains the files to be packaged together with the recipe.</param>
        /// <param name="executionId">An optional execution id for the operation. A new one will be generated if this is not provided.</param>
        /// <returns>The execution id for the import operation.</returns>
        string ImportRecipe(string recipeText, string filesPath = null, string executionId = null);
        /// <summary>
        /// Export a list of content items.
        /// </summary>
        /// <param name="contentTypes">The content types to export.</param>
        /// <param name="contentItems">The list of content items to export.</param>
        /// <param name="exportOptions">The options for the export operation.</param>
        /// <returns>A file path result for the export package.</returns>
        FilePathResult Export(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, ExportOptions exportOptions);
        /// <summary>
        /// Export a list of content items.
        /// </summary>
        /// <param name="contentTypes">The content types to export.</param>
        /// <param name="exportOptions">The options for the export operation.</param>
        /// <returns>A file path result for the export package.</returns>
        FilePathResult Export(IEnumerable<string> contentTypes, ExportOptions exportOptions);
    }
}
