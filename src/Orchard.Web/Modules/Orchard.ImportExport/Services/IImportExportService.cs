using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ImportExport.Models;
using Orchard.Recipes.Models;
using Orchard.Utility.Extensions;

namespace Orchard.ImportExport.Services {
    public interface IImportExportService : IDependency {
        string Import(ImportActionContext context, IEnumerable<IImportAction> actions = null);
        void Export(ExportActionContext context, IEnumerable<IExportAction> actions = null);
        string WriteExportFile(XDocument recipeDocument);
        IEnumerable<IExportAction> ParseExportActions(XDocument configurationDocument);
        void ConfigureImportActions(ConfigureImportActionsContext context);
    }

    public static class ImportExportServiceExtensions {

        public static string Import(this IImportExportService service, string recipeText) {
            var context = new ImportActionContext {
                RecipeDocument = XDocument.Parse(recipeText, LoadOptions.PreserveWhitespace)
            };
            service.Import(context);
            return context.ExecutionId;
        }

        public static string GetExportFileName(this Recipe recipe) {
            string format;

            if (String.IsNullOrWhiteSpace(recipe.Name) && String.IsNullOrWhiteSpace(recipe.Version))
                format = "export.xml";
            else if (String.IsNullOrWhiteSpace(recipe.Version))
                format = "{0}.recipe.xml";
            else if (String.IsNullOrWhiteSpace(recipe.Name))
                format = "export-{1}.recipe.xml";
            else
                format = "{0}-{1}.recipe.xml";

            return String.Format(format, recipe.Name.HtmlClassify(), recipe.Version);
        }
    }
}