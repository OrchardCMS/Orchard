using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.ContentManagement.Records {
    public class ContentItemRecord {
        public ContentItemRecord() {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            Versions = new List<ContentItemVersionRecord>();
// ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        public virtual int Id { get; set; }
        public virtual ContentTypeRecord ContentType { get; set; }
        public virtual IList<ContentItemVersionRecord> Versions { get; set; }

    }
}