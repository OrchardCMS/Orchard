using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.UI.Navigation;

namespace Orchard.Core.Containers.Models {
    public class ContainerPart : ContentPart<ContainerPartRecord> {
        public string ItemContentType {
            get { return Record.ItemContentType; }
            set { Record.ItemContentType = value; }
        }

        public bool ItemsShown {
            get { return Record.ItemsShown; }
            set { Record.ItemsShown = value; }
        }

        public bool Paginated {
            get { return Record.Paginated; }
            set { Record.Paginated = value; }
        }

        public int PageSize {
            get { return Record.PageSize; }
            set { Record.PageSize = value; }
        }

        public string OrderByProperty {
            get { return Record.OrderByProperty; }
            set { Record.OrderByProperty = value; }
        }

        public int OrderByDirection {
            get { return Record.OrderByDirection; }
            set { Record.OrderByDirection = value; }
        }

        public PagerParameters PagerParameters { get; set; }

        public ContainerPart() {
            PagerParameters = new PagerParameters();
        }
    }

    public class ContainerPartRecord : ContentPartRecord {
        public virtual string ItemContentType { get; set; }
        public virtual bool ItemsShown { get; set; }
        public virtual bool Paginated { get; set; }
        public virtual int PageSize { get; set; }
        public virtual string OrderByProperty { get; set; }
        public virtual int OrderByDirection { get; set; }
    }
}
