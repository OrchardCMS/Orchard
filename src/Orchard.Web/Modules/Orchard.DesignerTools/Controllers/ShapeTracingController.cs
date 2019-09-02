using System.IO;
using System.Web.Mvc;
using Orchard.FileSystems.WebSite;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using Orchard.ContentManagement;
using Orchard.DesignerTools.Models;

namespace Orchard.DesignerTools.Controllers {
    [Themed]
    public class ShapeTracingController : Controller {
        private readonly IOrchardServices _services;

        public ShapeTracingController(IOrchardServices orchardServices) {
            _services = orchardServices;
        }

        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult DeactivateTracing(string returnUrl) {
            _services.WorkContext.CurrentSite.As<ShapeTracingSiteSettingsPart>().IsShapeTracingActive = false;

            return this.RedirectLocal(returnUrl);
        }

        [HttpPost]
        public ActionResult ActivateTracing(string returnUrl) {
            _services.WorkContext.CurrentSite.As<ShapeTracingSiteSettingsPart>().IsShapeTracingActive = true;

            return this.RedirectLocal(returnUrl);
        }
    }
}