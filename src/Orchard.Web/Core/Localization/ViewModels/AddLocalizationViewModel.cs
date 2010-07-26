using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Localization.ViewModels {
    public class AddLocalizationViewModel : BaseViewModel {
        public int Id { get; set; }
        [Required]
        public string SelectedCulture { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public ContentItemViewModel Content { get; set; }
    }
}