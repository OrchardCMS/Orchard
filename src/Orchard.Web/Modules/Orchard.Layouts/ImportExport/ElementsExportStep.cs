using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Layouts.ImportExport {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }

    public class ElementsExportStep : ICustomExportStep {
        public void Register(IList<string> steps) {
            steps.Add("LayoutElements");
        }
    }
}