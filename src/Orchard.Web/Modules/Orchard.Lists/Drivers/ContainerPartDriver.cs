using Orchard.ContentManagement.Drivers;
using Orchard.Core.Containers.Models;

namespace Orchard.Lists.Drivers {
    public class ContainerPartDriver : ContentPartDriver<ContainerPart>{
        protected override DriverResult Display(ContainerPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Container_Contained_SummaryAdmin",
                             () => shapeHelper.Parts_Container_Contained_SummaryAdmin()
                );
        }
    }
}