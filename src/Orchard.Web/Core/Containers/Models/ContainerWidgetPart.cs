using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Containers.Models {
    public class ContainerWidgetPart : ContentPart<ContainerWidgetPartRecord> {
        public SelectList AvailableContainers { get; set; }
    }

    public class ContainerWidgetPartRecord : ContentPartRecord {
        public virtual int ContainerId { get; set; }
        public virtual int PageSize { get; set; }
        public virtual string OrderByProperty { get; set; }
        public virtual int OrderByDirection { get; set; }
    }
}