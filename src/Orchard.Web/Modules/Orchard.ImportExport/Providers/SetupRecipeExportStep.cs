using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;

namespace Orchard.ImportExport.Providers {
    public class SetupRecipeExportStep : ExportStepProvider {
        private readonly IOrchardServices _orchardServices;
        public SetupRecipeExportStep(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public override string Name {
            get { return "SetupRecipe"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Setup Recipe"); }
        }

        public override LocalizedString Description {
            get { return T("Turns the export file into a Setup recipe."); }
        }

        public override int Position { get { return -10; } }

        public string RecipeName { get; set; }
        public string RecipeDescription { get; set; }
        public string RecipeAuthor { get; set; }
        public string RecipeWebsite { get; set; }
        public string RecipeTags { get; set; }
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
                RecipeVersion = viewModel.RecipeVersion;
                IsSetupRecipe = true;
            }

            return shapeFactory.EditorTemplate(TemplateName: "ExportSteps/SetupRecipe", Model: viewModel, Prefix: Prefix);
        }

        public override void Export(ExportContext context) {
            var recipeElement = context.Document.Element("Orchard").Element("Recipe");
            
            recipeElement.SetElementValue("Name", RecipeName);
            recipeElement.SetElementValue("Description", RecipeDescription);
            recipeElement.SetElementValue("Author", RecipeAuthor);
            recipeElement.SetElementValue("WebSite", RecipeWebsite);
            recipeElement.SetElementValue("Tags", RecipeTags);
            recipeElement.SetElementValue("Version", RecipeVersion);
            recipeElement.SetElementValue("IsSetupRecipe", IsSetupRecipe);
        }
    }
}