using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Pages.Models {
    public class PageRecord : ContentPartRecord {
        public virtual DateTime? Published { get; set; }
    }
}
