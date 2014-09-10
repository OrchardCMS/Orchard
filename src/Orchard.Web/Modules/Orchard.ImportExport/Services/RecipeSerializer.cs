using System;
using System.Xml.Linq;
using Orchard.Recipes.Models;

namespace Orchard.ImportExport.Services {
    public class XmlRecipeSerializer : IRecipeSerializer {
        public string Serialize(Recipe recipe) {
            var exportDocument = new XDocument(
                new XDeclaration("1.0", "", "yes"),
                new XComment("Exported from Orchard"),
                new XElement("Orchard",
                    new XElement("Recipe",
                        new XElement("Name", recipe.Name),
                        new XElement("Author", recipe.Author),
                        new XElement("ExportUtc", recipe.ExportUtc),
                        new XElement("Description", recipe.Description),
                        new XElement("WebSite", recipe.WebSite),
                        new XElement("Version", recipe.Version),
                        new XElement("Tags", recipe.Tags)
                        )
                    )
                );

            var orchardElement = exportDocument.Element("Orchard");
            if (orchardElement == null) {
                throw new InvalidOperationException();
            }

            foreach (var step in recipe.RecipeSteps) {
                orchardElement.Add(step.Step);
            }

            return exportDocument.ToString();
        }
    }
}
