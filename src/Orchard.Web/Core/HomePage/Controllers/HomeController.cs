using System;
using System.Collections.Generic;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Settings;
using Orchard.Themes;

namespace Orchard.Core.HomePage.Controllers {
    [HandleError]
    public class HomeController : Controller {
        private readonly IEnumerable<IHomePageProvider> _homePageProviders;

        public HomeController(IEnumerable<IHomePageProvider> homePageProviders) {
            _homePageProviders = homePageProviders;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        [Themed]
        public ActionResult Index() {
            try {
                var homepage = CurrentSite.HomePage;
                if (String.IsNullOrEmpty(homepage))
                    return View();

                var homePageParameters = homepage.Split(';');
                if (homePageParameters.Length != 2)
                    return View();

                var providerName = homePageParameters[0];
                var item = Int32.Parse(homePageParameters[1]);

                foreach (var provider in _homePageProviders) {
                    if (!string.Equals(provider.GetProviderName(), providerName))
                        continue;

                    var result = provider.GetHomePage(item);
                    if (result is ViewResultBase) {
                        var resultBase = result as ViewResultBase;
                        ViewData.Model = resultBase.ViewData.Model;
                        resultBase.ViewData = ViewData;
                    }

                    return result;
                }

                return View();
            }
            catch {
                return View();
            }
        }
    }

}