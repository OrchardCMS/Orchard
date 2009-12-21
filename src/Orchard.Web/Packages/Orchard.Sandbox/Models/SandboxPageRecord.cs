using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace Orchard.Sandbox.Models {
    public class SandboxPageRecord : ContentPartRecord{
        public virtual string Name { get; set; }
    }
}
