using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeParser : IRecipeParser {
        public RecipeParser() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public Recipe ParseRecipe(string recipeText) {
            var recipe = new Recipe();
            
            try {
                var recipeSteps = new List<RecipeStep>();
                TextReader textReader = new StringReader(recipeText);
                var recipeTree = XElement.Load(textReader, LoadOptions.PreserveWhitespace);
                textReader.Close();

                foreach (var element in recipeTree.Elements()) {
                    // Recipe mETaDaTA
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
                                case "Tags":
                                    recipe.Tags = metadataElement.Value;
                                    break;
                                default:
                                    Logger.Error("Unrecognized recipe metadata element {0} encountered. Skipping.", metadataElement.Name.LocalName);
                                    break;
                            }
                        }
                    }
                    // Recipe step
                    else {
                        var recipeStep = new RecipeStep { Name = element.Name.LocalName, Step = element };
                        recipeSteps.Add(recipeStep);
                    }
                }
                recipe.RecipeSteps = recipeSteps;
            }
            catch (Exception exception) {
                Logger.Error(exception, "Parsing recipe failed. Recipe text was: {0}.", recipeText);
                throw;
            }

            return recipe;
        }
    }
}