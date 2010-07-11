using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Common.Fields {
    public class ItemReferenceContentField : ContentField {
        public ContentItemRecord ContentItemReference { get; set; }
    }
}
