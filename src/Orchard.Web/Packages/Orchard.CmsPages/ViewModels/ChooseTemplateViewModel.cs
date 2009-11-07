using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.CmsPages.Services.Templates;
using Orchard.Mvc.ViewModels;

namespace Orchard.CmsPages.ViewModels {
    public class ChooseTemplateViewModel : AdminViewModel {
        public string TemplateName { get; set; }
        public IList<TemplateDescriptor> Templates{ get; set; }
    }
}
