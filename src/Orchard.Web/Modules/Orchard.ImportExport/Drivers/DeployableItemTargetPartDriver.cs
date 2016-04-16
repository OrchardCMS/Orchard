using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Drivers {
    [OrchardFeature("Orchard.Deployment")]
    public class DeployableItemTargetPartDriver : ContentPartDriver<DeployableItemTargetPart> {
    }
}