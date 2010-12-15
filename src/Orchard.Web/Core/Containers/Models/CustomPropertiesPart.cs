using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Containers.Models {
    public class CustomPropertiesPart : ContentPart<CustomPropertiesPartRecord> {
    }

    public class CustomPropertiesPartRecord : ContentPartRecord {
        public virtual string CustomOne { get; set; }
        public virtual string CustomTwo { get; set; }
        public virtual string CustomThree { get; set; }
    }
}
