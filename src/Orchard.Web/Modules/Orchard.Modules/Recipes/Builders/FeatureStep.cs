using System;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Modules.ViewModels;
using Orchard.Recipes.Services;

namespace Orchard.Modules.Recipes.Builders {
    public class FeatureStep : RecipeBuilderStep {
        private readonly IFeatureManager _featureManager;

        public FeatureStep(IFeatureManager featureManager) {
            _featureManager = featureManager;
        }

        public override string Name {
            get { return "Feature"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Features"); }
        }

        public override LocalizedString Description {
            get { return T("Exports enabled and disabled features."); }
        }

        public override int Priority { get { return 0; } }

        public bool ExportEnabledFeatures { get; set; }
        public bool ExportDisabledFeatures { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new FeatureStepViewModel();

            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                ExportEnabledFeatures = viewModel.ExportEnabledFeatures;
                ExportDisabledFeatures = viewModel.ExportDisabledFeatures;
            }

            return shapeFactory.EditorTemplate(TemplateName: "ExportSteps/Feature", Model: viewModel, Prefix: Prefix);
        }

        public override void Build(BuildContext context) {
            if (!ExportEnabledFeatures && !ExportDisabledFeatures)
                return;

            var enabledFeatures = _featureManager.GetEnabledFeatures();
            var disabledFeatures = _featureManager.GetDisabledFeatures();
            var orchardElement = context.RecipeDocument.Element("Orchard");
            var root = new XElement("Feature");

            if(ExportEnabledFeatures)
                root.Add(new XAttribute("enable", String.Join(", ", enabledFeatures.Select(x => x.Id).OrderBy(x => x))));

            if (ExportDisabledFeatures)
                root.Add(new XAttribute("disable", String.Join(", ", disabledFeatures.Select(x => x.Id).OrderBy(x => x))));

            orchardElement.Add(root);
        }
    }
}