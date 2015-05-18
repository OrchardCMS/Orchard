using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Services;

namespace Orchard.AuditTrail.ImportExport {
    [OrchardFeature("Orchard.AuditTrail.ImportExport")]
    public class AuditTrailExportStep : ICustomExportStep {
        public void Register(IList<string> steps) {
            steps.Add("AuditTrail");
        }
    }
}