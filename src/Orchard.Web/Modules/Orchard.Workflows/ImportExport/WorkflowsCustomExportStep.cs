using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Workflows.ImportExport {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }

    public class WorkflowsCustomExportStep : ICustomExportStep {
        public void Register(IList<string> steps) {
            steps.Add("Workflows");
        }
    }
}