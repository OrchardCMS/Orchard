using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace Orchard.AntiSpam.Models {
    public class SpamFilterPartRecord : ContentPartRecord {
        public virtual SpamStatus Status { get; set; }
    }
}