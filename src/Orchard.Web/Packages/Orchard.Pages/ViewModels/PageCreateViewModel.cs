using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.Pages.Services.Templates;
using Orchard.Mvc.ViewModels;

namespace Orchard.Pages.ViewModels {
    public class PageCreateViewModel : AdminViewModel {
        [Required, DisplayName("Title:")]
        public string Title { get; set; }

        [Required, DisplayName("Permalink:")]
        public string Slug { get; set; }

        [DisplayName("Template:")]
        public string TemplateName { get; set; }

        public IList<TemplateDescriptor> Templates { get; set; }
    }
}
