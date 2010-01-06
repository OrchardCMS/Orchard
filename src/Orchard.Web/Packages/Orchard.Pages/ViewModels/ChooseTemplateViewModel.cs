using System.Collections.Generic;
using Orchard.Pages.Services.Templates;
using Orchard.Mvc.ViewModels;

namespace Orchard.Pages.ViewModels {
    public class ChooseTemplateViewModel : AdminViewModel {
        public string TemplateName { get; set; }
        public IList<TemplateDescriptor> Templates{ get; set; }
    }
}
