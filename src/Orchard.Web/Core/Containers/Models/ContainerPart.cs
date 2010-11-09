using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Containers.Models {
    public class ContainerPart : ContentPart<ContainerPartRecord> {
    }

    public class ContainerPartRecord : ContentPartRecord {
        public virtual int Paginated { get; set; }
        public virtual int PageSize { get; set; }
        public virtual string OrderByProperty { get; set; }
        public virtual int OrderByDirection { get; set; }
    }

    public enum OrderByDirection { 
        Ascending,
        Descending,
    }
}
