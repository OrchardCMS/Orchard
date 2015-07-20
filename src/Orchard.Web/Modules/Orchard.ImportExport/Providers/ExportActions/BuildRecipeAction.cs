using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IRecipeBuilder _recipeBuilder;

        public BuildRecipeAction(IEnumerable<IRecipeBuilderStep> recipeBuilderSteps, IRecipeBuilder recipeBuilder) {
            _recipeBuilderSteps = recipeBuilderSteps;
            _recipeBuilder = recipeBuilder;

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
            RecipeBuilderSteps.Clear();

            var recipeBuilderStepsElement = context.ConfigurationElement.Element("Steps");
            if (recipeBuilderStepsElement == null)
                return;

            foreach (var stepElement in recipeBuilderStepsElement.Elements()) {
                var step = _recipeBuilderSteps.SingleOrDefault(x => x.Name == stepElement.Name.LocalName);

                if (step != null) {
                    var stepContext = new RecipeBuilderStepConfigurationContext(stepElement);
                    step.Configure(stepContext);
                    RecipeBuilderSteps.Add(step);
                }
            }
        }

        public override void Execute(ExportActionContext context) {
            context.RecipeDocument = _recipeBuilder.Build(RecipeBuilderSteps);
        }
    }
}