using System.Collections.Generic;

namespace Orchard.ImportExport.ViewModels {
    public class ExportViewModel {
        public ExportViewModel() {
            Actions = new List<ExportActionViewModel>();
        }

        public IList<ExportActionViewModel> Actions { get; set; }
    }
}
