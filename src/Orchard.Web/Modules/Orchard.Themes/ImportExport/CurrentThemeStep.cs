using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Events;

namespace Orchard.Themes.ImportExport {
    public interface ICustomExportStep : IEventHandler {
        void Register(IList<string> steps);
    }

    [OrchardFeature("Orchard.Themes.ImportExportCurrentTheme")]
    public class CurrentThemeStep : ICustomExportStep {

        void ICustomExportStep.Register(IList<string> steps) {
            steps.Add("CurrentTheme");
        }
    }
}

