using System;
using System.Collections.Generic;

namespace Orchard.ImportExport.Models {
    [Obsolete]
    public class ExportOptions {
        public IEnumerable<string> CustomSteps { get; set; }
    }
}