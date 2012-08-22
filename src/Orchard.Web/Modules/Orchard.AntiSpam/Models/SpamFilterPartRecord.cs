using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.AntiSpam.Models {
    public class SpamFilterPart : ContentPart<SpamFilterPartRecord> {

        public SpamStatus Status {
            get { return Record.Status; }
            set { Record.Status = value; }
        }
    }
}