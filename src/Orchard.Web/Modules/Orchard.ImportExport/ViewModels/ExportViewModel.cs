using System.Collections.Generic;

namespace Orchard.ImportExport.ViewModels {
    public class ExportViewModel {
        public IList<CustomStepEntry> CustomSteps { get; set; }
        public IList<ExportStepViewModel> ExportSteps { get; set; }
    }
}
