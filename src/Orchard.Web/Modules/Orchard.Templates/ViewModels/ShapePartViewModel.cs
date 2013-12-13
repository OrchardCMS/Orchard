using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Templates.ViewModels {
    public class ShapePartViewModel {
        [StringLength(100)]
        public string Name { get; set; }
        [UIHint("TemplateLanguagePicker")]
        public string Language { get; set; }
        public string Template { get; set; }
        public IEnumerable<string> AvailableLanguages { get; set; }
    }
}