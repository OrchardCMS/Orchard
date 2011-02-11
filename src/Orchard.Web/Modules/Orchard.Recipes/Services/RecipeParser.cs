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
        ILogger Logger { get; set; }

        public Recipe ParseRecipe(string recipeText) {
            var recipe = new Recipe();
            
            try {
                var recipeSteps = new List<RecipeStep>();
                TextReader textReader = new StringReader(recipeText);
                var recipeTree = XElement.Load(textReader, LoadOptions.PreserveWhitespace);
                textReader.Close();

                foreach (var element in recipeTree.Elements()) {
                    switch(element.Name.ToString()) {
                        case "Name":
                            recipe.Name = element.Value;
                            break;
                        case "Description":
                            recipe.Description = element.Value;
                            break;
                        case "Author":
                            recipe.Author = element.Value;
                            break;
                        case "WebSite":
                            recipe.WebSite = element.Value;
                            break;
                        case "Version":
                            recipe.Version = element.Value;
                            break;
                        case "Tags":
                            recipe.Tags = element.Value;
                            break;
                        default:
                            var recipeStep = new RecipeStep {Name = element.Name.ToString(), Step = element};
                            recipeSteps.Add(recipeStep);
                            break;
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