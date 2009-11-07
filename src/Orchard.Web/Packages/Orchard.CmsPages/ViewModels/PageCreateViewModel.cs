using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.CmsPages.Services.Templates;
using Orchard.Mvc.ViewModels;

namespace Orchard.CmsPages.ViewModels {
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
