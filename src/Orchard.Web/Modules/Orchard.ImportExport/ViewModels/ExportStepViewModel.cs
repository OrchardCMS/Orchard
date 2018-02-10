using Orchard.Localization;

namespace Orchard.ImportExport.ViewModels {
    public class ExportStepViewModel {
        public string Name { get; set; }
        public LocalizedString DisplayName { get; set; }
        public LocalizedString Description { get; set; }
        public bool IsSelected { get; set; }
        public dynamic Editor { get; set; }
        public bool IsVisible { get; set; }
    }
}