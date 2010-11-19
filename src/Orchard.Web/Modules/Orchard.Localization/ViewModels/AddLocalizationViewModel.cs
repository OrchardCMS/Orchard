using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Localization.ViewModels {
    public class AddLocalizationViewModel  {
        public int Id { get; set; }
        [Required]
        public string SelectedCulture { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public IContent Content { get; set; }
    }
}