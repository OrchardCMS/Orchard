using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Events;

namespace Orchard.DynamicForms.ImportExport {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }

    [OrchardFeature("Orchard.DynamicForms.ImportExport")]
    public class FormsExportStep : ICustomExportStep {
        public void Register(IList<string> steps) {
            steps.Add("Forms");
        }
    }
}