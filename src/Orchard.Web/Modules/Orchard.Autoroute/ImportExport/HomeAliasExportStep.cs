using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Autoroute.ImportExport {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }

    public class HomeAliasExportStep : ICustomExportStep {
        public void Register(IList<string> steps) {
            steps.Add("HomeAlias");
        }
    }
}