using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Containers.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Settings;
using Orchard.Core.Feeds;
using Orchard.Environment.Extensions;

namespace Orchard.Lists.Drivers {
    [OrchardSuppressDependency("Orchard.Core.Containers.Drivers.ContainerPartDriver")]
    public class ContainerPartDriver : Orchard.Core.Containers.Drivers.ContainerPartDriver {
        public ContainerPartDriver(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            ISiteService siteService,
            IFeedManager feedManager)
            : base(contentDefinitionManager, orchardServices, siteService, feedManager) {
        }

        protected override DriverResult Display(ContainerPart part, string displayType, dynamic shapeHelper) {
            var shape = base.Display(part, displayType, (object)shapeHelper); // shapeHelper casting to avoid compiler error

            if (shape == null) return null;

            return Combined(
                shape,
                ContentShape("Parts_Container_Contained_SummaryAdmin",
                             () => shapeHelper.Parts_Container_Contained_SummaryAdmin(ContentPart: part))
                );
        }
    }
}