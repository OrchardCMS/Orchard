using System.Collections.Generic;

namespace Orchard.ImportExport.ViewModels {
    public class ImportViewModel {
        public ImportViewModel() {
            Actions = new List<ImportActionViewModel>();
        }

        public IList<ImportActionViewModel> Actions { get; set; }
    }
}