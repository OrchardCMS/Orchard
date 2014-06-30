using System.Linq;
using System.Xml.Linq;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;

namespace Orchard.ImportExport.Handlers {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentMetadataExportEventHandler : IExportEventHandler {
        public const string StepName = "DeploymentMeta";

        public void Exporting(ExportContext context) {
            //Not required
        }

        public void Exported(ExportContext context) {
            var deploymentMetaSteps = context.ExportOptions.CustomSteps
                .Select(DeploymentMetadata.FromExportStep)
                .Where(c => c != null);

            if (!deploymentMetaSteps.Any())
                return;

            var recipeDescription = string.Join(";", deploymentMetaSteps.Select(m => m.ToDisplayString()));
            context.Document.Element("Orchard").Element("Recipe").Add(new XElement("Description", recipeDescription));
        }
    }
}