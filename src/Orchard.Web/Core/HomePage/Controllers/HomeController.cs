using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Themes;
using Orchard.Localization;

namespace Orchard.Core.HomePage.Controllers {
    [HandleError]
    public class HomeController : Controller {

        public HomeController() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [Themed]
        public ActionResult Index() {
            Logger.Error(T("No home page route exists").Text);
            return View();
        }

    }
}