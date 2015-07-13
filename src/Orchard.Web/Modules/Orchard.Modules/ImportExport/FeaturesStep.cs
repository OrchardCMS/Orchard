using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Modules.ImportExport {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }

    public class FeaturesStep : ICustomExportStep {
        public void Register(IList<string> steps) {
            steps.Add("EnabledFeatures");
        }
    }
}