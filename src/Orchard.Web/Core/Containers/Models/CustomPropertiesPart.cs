using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Containers.Models {
    [Obsolete("Use content fields instead.")]
    public class CustomPropertiesPart : ContentPart<CustomPropertiesPartRecord> {
    }

    [Obsolete("Use content fields instead.")]
    public class CustomPropertiesPartRecord : ContentPartRecord {
        public virtual string CustomOne { get; set; }
        public virtual string CustomTwo { get; set; }
        public virtual string CustomThree { get; set; }
    }
}
