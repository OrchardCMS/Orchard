using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeParser : Component, IRecipeParser {

        public Recipe ParseRecipe(XDocument recipeDocument) {
            var recipe = new Recipe();
            var recipeSteps = new List<RecipeStep>();
            var stepId = 0;

            foreach (var element in recipeDocument.Root.Elements()) {
                // Recipe metadata.
                if (element.Name.LocalName == "Recipe") {
                    foreach (var metadataElement in element.Elements()) {
                        switch (metadataElement.Name.LocalName) {
                            case "Name":
                                recipe.Name = metadataElement.Value;
                                break;
                            case "Description":
                                recipe.Description = metadataElement.Value;
                                break;
                            case "Author":
                                recipe.Author = metadataElement.Value;
                                break;
                            case "WebSite":
                                recipe.WebSite = metadataElement.Value;
                                break;
                            case "Version":
                                recipe.Version = metadataElement.Value;
                                break;
                            case "IsSetupRecipe":
                                recipe.IsSetupRecipe = !string.IsNullOrEmpty(metadataElement.Value) ? bool.Parse(metadataElement.Value) : false;
                                break;
                            case "ExportUtc":
                                recipe.ExportUtc = !string.IsNullOrEmpty(metadataElement.Value) ? (DateTime?)XmlConvert.ToDateTime(metadataElement.Value, XmlDateTimeSerializationMode.Utc) : null;
                                break;
                            case "Category":
                                recipe.Category = metadataElement.Value;
                                break;
                            case "Tags":
                                recipe.Tags = metadataElement.Value;
                                break;
                            default:
                                Logger.Warning("Unrecognized recipe metadata element '{0}' encountered; skipping.", metadataElement.Name.LocalName);
                                break;
                        }
                    }
                }
                // Recipe step.
                else {
                    var recipeStep = new RecipeStep(id: (++stepId).ToString(CultureInfo.InvariantCulture), recipeName: recipe.Name, name: element.Name.LocalName, step: element );
                    recipeSteps.Add(recipeStep);
                }
            }

            recipe.RecipeSteps = recipeSteps;

            return recipe;
        }
    }
}