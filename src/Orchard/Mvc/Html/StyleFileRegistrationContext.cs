using System.Web.Mvc;

namespace Orchard.Mvc.Html {
    public class StyleFileRegistrationContext : FileRegistrationContext {
        public StyleFileRegistrationContext(ControllerContext viewContext, IViewDataContainer viewDataContainer, string fileName)
            : base(viewContext, viewDataContainer, fileName) {
        }

        public string Media { get; set; }
    }
}