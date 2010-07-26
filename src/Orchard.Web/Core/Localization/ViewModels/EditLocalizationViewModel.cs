using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Localization.ViewModels {
    public class EditLocalizationViewModel : BaseViewModel {
        public string SelectedCulture { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public IContent ContentItem { get; set; }
        public IContent MasterContentItem { get; set; }
        public ContentLocalizationsViewModel ContentLocalizations { get; set; }
    }
}