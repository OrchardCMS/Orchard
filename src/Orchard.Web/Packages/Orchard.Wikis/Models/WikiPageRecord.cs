using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Models.Records;

namespace Orchard.Wikis.Models {
    public class WikiPageRecord : ContentPartRecord{
        public virtual string Name { get; set; }
    }
}
