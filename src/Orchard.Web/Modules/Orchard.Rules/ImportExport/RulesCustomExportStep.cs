using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Rules.ImportExport {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }

    public class RulesCustomExportStep : ICustomExportStep {
        public void Register(IList<string> steps) {
            steps.Add("Rules");
        }
    }
}