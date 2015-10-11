using System.Collections.Generic;

namespace Orchard.ImportExport.ViewModels {
    public class RecipeBuilderViewModel {
        public bool UploadConfigurationFile { get; set; }
        public IList<ExportStepViewModel> Steps { get; set; }
    }
}