using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Mvc;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.Providers.ExportActions {
    public class BuildRecipeAction : ExportAction {
        private readonly IEnumerable<IRecipeBuilderStep> _recipeBuilderSteps;
        private readonly IRecipeBuilderStepResolver _recipeBuilderStepResolver;
        private readonly IRecipeBuilder _recipeBuilder;
        private readonly IOrchardServices _orchardServices;

        public BuildRecipeAction(IRecipeBuilderStepResolver recipeBuilderStepResolver, IEnumerable<IRecipeBuilderStep> recipeBuilderSteps, IRecipeBuilder recipeBuilder, IOrchardServices orchardServices) {
            _recipeBuilderSteps = recipeBuilderSteps;
            _recipeBuilderStepResolver = recipeBuilderStepResolver;
            _recipeBuilder = recipeBuilder;
            _orchardServices = orchardServices;

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
                Editor = x.BuildEditor(shapeFactory),
                IsVisible = x.IsVisible
            });

            var viewModel = new RecipeBuilderViewModel {
                Steps = builderSteps.ToList()
            };

            if (updater != null) {
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    if (viewModel.UploadConfigurationFile) {
                        var configurationFile = _orchardServices.WorkContext.HttpContext.Request.Files["ConfigurationFile"];

                        if (configurationFile.ContentLength == 0)
                            updater.AddModelError("ConfigurationFile", T("No configuration file was specified."));
                        else {
                            var configurationDocument = XDocument.Parse(new StreamReader(configurationFile.InputStream).ReadToEnd());
                            Configure(new ExportActionConfigurationContext(configurationDocument.Root.Element(Name)));
                        }
                    }
                    else {
                        var exportStepNames = viewModel.Steps.Where(x => x.IsSelected).Select(x => x.Name);
                        var stepsQuery = _recipeBuilderStepResolver.Resolve(exportStepNames);
                        var steps = stepsQuery.ToArray();
                        var stepUpdater = new Updater(updater, secondHalf => String.Format("{0}.{1}", Prefix, secondHalf));
                        foreach (var exportStep in steps) {
                            exportStep.UpdateEditor(shapeFactory, stepUpdater);
                        }

                        RecipeBuilderSteps = steps;
                    }
                }
            }

            return shapeFactory.EditorTemplate(TemplateName: "ExportActions/BuildRecipe", Model: viewModel, Prefix: Prefix);
        }

        public override void Configure(ExportActionConfigurationContext context) {
            RecipeBuilderSteps.Clear();

            var recipeBuilderStepsElement = context.ConfigurationElement.Element("Steps");
            if (recipeBuilderStepsElement == null)
                return;

            foreach (var stepElement in recipeBuilderStepsElement.Elements()) {
                var step = _recipeBuilderStepResolver.Resolve(stepElement.Name.LocalName);

                if (step != null) {
                    var stepContext = new RecipeBuilderStepConfigurationContext(stepElement);
                    step.Configure(stepContext);
                    RecipeBuilderSteps.Add(step);
                }
            }
        }

        public override void ConfigureDefault() {
            RecipeBuilderSteps = _recipeBuilderSteps.ToList();

            foreach (var step in RecipeBuilderSteps) {
                step.ConfigureDefault();
            }
        }

        public override void Execute(ExportActionContext context) {
            context.RecipeDocument = _recipeBuilder.Build(RecipeBuilderSteps);
        }
    }
}