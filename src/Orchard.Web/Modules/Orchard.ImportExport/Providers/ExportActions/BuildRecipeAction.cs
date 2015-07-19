using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Mvc;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Utility.Extensions;

namespace Orchard.ImportExport.Providers.ExportActions {
    public class BuildRecipeAction : ExportAction {
        private readonly IEnumerable<IRecipeBuilderStep> _recipeBuilderSteps;
        private readonly IImportExportService _importExportService;
        private readonly IRecipeParser _recipeParser;

        public BuildRecipeAction(IEnumerable<IRecipeBuilderStep> recipeBuilderSteps, IImportExportService importExportService, IRecipeParser recipeParser) {
            _recipeBuilderSteps = recipeBuilderSteps;
            _importExportService = importExportService;
            _recipeParser = recipeParser;

            RecipeBuilderSteps = new List<IRecipeBuilderStep>();
        }

        public override string Name { get { return "BuildRecipe"; } }

        public IList<IRecipeBuilderStep> RecipeBuilderSteps { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var builderSteps = _recipeBuilderSteps.OrderBy(x => x.Position).Select(x => new ExportStepViewModel {
                Name = x.Name,
                DisplayName = x.DisplayName,
                Description = x.Description,
                Editor = x.BuildEditor(shapeFactory)
            });

            var viewModel = new RecipeBuilderViewModel {
                Steps = builderSteps.ToList()
            };

            if (updater != null) {
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    var exportStepNames = viewModel.Steps.Where(x => x.IsSelected).Select(x => x.Name);
                    var stepsQuery = from name in exportStepNames
                                     let provider = _recipeBuilderSteps.SingleOrDefault(x => x.Name == name)
                                     where provider != null
                                     select provider;
                    var steps = stepsQuery.ToArray();
                    var stepUpdater = new Updater(updater, secondHalf => String.Format("{0}.{1}", Prefix, secondHalf));
                    foreach (var exportStep in steps) {
                        exportStep.UpdateEditor(shapeFactory, stepUpdater);
                    }

                    RecipeBuilderSteps = steps;
                }
            }

            return shapeFactory.EditorTemplate(TemplateName: "ExportActions/BuildRecipe", Model: viewModel, Prefix: Prefix);
        }

        public override void Configure(ExportActionConfigurationContext context) {
            var recipeBuilderStepsElement = context.ConfigurationElement.Element("RecipeBuilderSteps");
            if (recipeBuilderStepsElement == null)
                return;

            foreach (var step in _recipeBuilderSteps) {
                var stepConfigurationElement = recipeBuilderStepsElement.Element(step.Name);

                if (stepConfigurationElement != null) {
                    var stepContext = new RecipeBuilderStepConfigurationContext(stepConfigurationElement);
                    step.Configure(stepContext);
                }
            }
        }

        public override void Execute(ExportActionContext context) {
            var recipeDocument = _importExportService.Export(RecipeBuilderSteps);
            var recipe = _recipeParser.ParseRecipe(recipeDocument);
            var exportFileName = GetExportFileName(recipe);
            var exportFilePath = _importExportService.WriteExportFile(recipeDocument);
            var actionResult = new FilePathResult(exportFilePath, "text/xml");

            actionResult.FileDownloadName = exportFileName;
            context.ActionResult = actionResult;
        }

        private string GetExportFileName(Recipe recipe) {
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