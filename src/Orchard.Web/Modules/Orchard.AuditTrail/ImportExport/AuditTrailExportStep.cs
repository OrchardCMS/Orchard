using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.AuditTrail.ImportExport {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }

    public class AuditTrailExportStep : ICustomExportStep {
        public void Register(IList<string> steps) {
            steps.Add("AuditTrail");
        }
    }
}