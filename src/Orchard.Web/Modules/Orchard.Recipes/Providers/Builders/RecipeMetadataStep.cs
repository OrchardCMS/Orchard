using System.Xml;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Recipes.ViewModels;

namespace Orchard.Recipes.Providers.Builders {
    public class RecipeMetadataStep : RecipeBuilderStep {
        private readonly IOrchardServices _orchardServices;
        public RecipeMetadataStep(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public override string Name {
            get { return "RecipeMetadata"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Recipe Metadata"); }
        }

        public override LocalizedString Description {
            get { return T("Provides additional information about the recipe."); }
        }

        public override int Priority { get { return 1000; } }
        public override int Position { get { return -1000; } }

        public string RecipeName { get; set; }
        public string RecipeDescription { get; set; }
        public string RecipeAuthor { get; set; }
        public string RecipeWebsite { get; set; }
        public string RecipeTags { get; set; }
        public string RecipeCategory { get; set; }
        public string RecipeVersion { get; set; }
        public bool IsSetupRecipe { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new SetupRecipeStepViewModel {
                RecipeAuthor = _orchardServices.WorkContext.CurrentUser.UserName
            };

            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                RecipeName = viewModel.RecipeName;
                RecipeDescription = viewModel.RecipeDescription;
                RecipeAuthor = viewModel.RecipeAuthor;
                RecipeWebsite = viewModel.RecipeWebsite;
                RecipeTags = viewModel.RecipeTags;
                RecipeCategory = viewModel.RecipeCategory;
                RecipeVersion = viewModel.RecipeVersion;
                IsSetupRecipe = viewModel.IsSetupRecipe;
            }

            return shapeFactory.EditorTemplate(TemplateName: "BuilderSteps/RecipeMetadata", Model: viewModel, Prefix: Prefix);
        }

        public override void Configure(RecipeBuilderStepConfigurationContext context) {
            RecipeName = context.ConfigurationElement.Attr("Name");
            RecipeDescription = context.ConfigurationElement.Attr("Description");
            RecipeAuthor = context.ConfigurationElement.Attr("Author");
            RecipeWebsite = context.ConfigurationElement.Attr("Website");
            RecipeTags = context.ConfigurationElement.Attr("Tags");
            RecipeCategory = context.ConfigurationElement.Attr("Category");
            RecipeVersion = context.ConfigurationElement.Attr("Version");
            IsSetupRecipe = context.ConfigurationElement.Attr<bool>("IsSetupRecipe");
        }

        public override void Build(BuildContext context) {
            var recipeElement = context.RecipeDocument.Element("Orchard").Element("Recipe");
            
            recipeElement.SetElementValue("Name", RecipeName);
            recipeElement.SetElementValue("Description", RecipeDescription);
            recipeElement.SetElementValue("Author", RecipeAuthor);
            recipeElement.SetElementValue("WebSite", RecipeWebsite);
            recipeElement.SetElementValue("Tags", RecipeTags);
            recipeElement.SetElementValue("Category", RecipeCategory);
            recipeElement.SetElementValue("Version", RecipeVersion);
            recipeElement.SetElementValue("IsSetupRecipe", IsSetupRecipe);
        }
    }
}