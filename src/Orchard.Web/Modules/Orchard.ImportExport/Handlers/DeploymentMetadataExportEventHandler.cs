using System;
using System.Linq;
using System.Xml.Linq;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Localization;

namespace Orchard.ImportExport.Handlers {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentMetadataExportEventHandler : IExportEventHandler {
        public const string StepName = "DeploymentMeta";

        public DeploymentMetadataExportEventHandler() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Exporting(ExportContext context) {
            //Not required
        }

        public void Exported(ExportContext context) {
            var deploymentMetaSteps = context.ExportOptions.CustomSteps
                .Select(DeploymentMetadata.FromExportStep)
                .Where(c => c != null)
                .Select(m => m.ToDisplayString())
                .ToList();

            if (!deploymentMetaSteps.Any()) return;

            var recipeDescription = string.Join(";", deploymentMetaSteps);
            var orchardElement = context.Document.Element("Orchard");
            if (orchardElement == null) {
                throw new InvalidOperationException(T("Recipe document does not have a top-level Orchard element.").Text);
            }
            var recipeElement = orchardElement.Element("Recipe");
            if (recipeElement == null) {
                throw new InvalidOperationException(T("Recipe document does not have a recipe element under the Orchard element.").Text);
            }
            recipeElement.Add(new XElement("Description", recipeDescription));
        }
    }
}
