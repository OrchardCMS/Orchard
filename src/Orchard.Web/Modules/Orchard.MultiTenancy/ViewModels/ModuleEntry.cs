using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.MultiTenancy.ViewModels {
    public class ModuleEntry {
        public bool Checked { get; set; }
        public string ModuleName { get; set; }
        public string ModuleId { get; set; }
    }
}