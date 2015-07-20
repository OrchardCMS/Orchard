using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.Recipes.Builders {
    [Obsolete("Implement IRecipeBuilderStep and IRecipeExecutionStep instead of implementing custom export steps.")]
    public class CustomStepsStep : RecipeBuilderStep {
        private readonly IEnumerable<IExportEventHandler> _exportEventHandlers;
        private readonly ICustomExportStep _customExportStep;

        public CustomStepsStep(IEnumerable<IExportEventHandler> exportEventHandlers, ICustomExportStep customExportStep) {
            _exportEventHandlers = exportEventHandlers;
            _customExportStep = customExportStep;
            CustomSteps = new List<string>();
        }

        public override string Name {
            get { return "CustomSteps"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Additional Export Steps"); }
        }

        public override LocalizedString Description {
            get { return T("Exports additional items."); }
        }

        public override bool IsVisible {
            get { return CustomSteps.Any(); }
        }

        public override int Priority { get { return -50; } }
        public override int Position { get { return 500; } }

        public IList<string> CustomSteps { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var customSteps = new List<string>();
            _customExportStep.Register(customSteps);

            var viewModel = new CustomStepsViewModel {
                CustomSteps = customSteps.Select(x => new CustomStepEntry { CustomStep = x}).ToList()
            };

            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                CustomSteps = viewModel.CustomSteps.Where(x => x.IsChecked).Select(x => x.CustomStep).ToList();
            }

            return shapeFactory.EditorTemplate(TemplateName: "BuilderSteps/CustomSteps", Model: viewModel, Prefix: Prefix);
        }

        public override void Configure(RecipeBuilderStepConfigurationContext context) {
            var steps = (context.ConfigurationElement.Attr("Steps") ?? "").Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            CustomSteps = steps.ToList();
        }

        public override void ConfigureDefault() {
            _customExportStep.Register(CustomSteps);
        }

        public override void Build(BuildContext context) {
            var exportContext = new ExportContext {
                Document = context.RecipeDocument,
                ExportOptions = new ExportOptions {
                    CustomSteps = CustomSteps
                }
            };
            _exportEventHandlers.Invoke(x => x.Exporting(exportContext), Logger);
            _exportEventHandlers.Invoke(x => x.Exported(exportContext), Logger);
        }
    }
}