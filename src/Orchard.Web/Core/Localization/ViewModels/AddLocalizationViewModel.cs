using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Localization.ViewModels {
    public class AddLocalizationViewModel : BaseViewModel {
        public int Id { get; set; }
        public string SelectedCulture { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public ContentItemViewModel Content { get; set; }
    }
}