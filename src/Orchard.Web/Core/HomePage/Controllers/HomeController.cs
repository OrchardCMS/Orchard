using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Themes;

namespace Orchard.Core.HomePage.Controllers {
    [HandleError]
    public class HomeController : Controller {
        private readonly IEnumerable<IHomePageProvider> _homePageProviders;
        private readonly IOrchardServices _orchardServices;

        public HomeController(IEnumerable<IHomePageProvider> homePageProviders, IOrchardServices orchardServices) {
            _homePageProviders = homePageProviders;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [Themed]
        public ActionResult Index() {
            try {
                var homepage = _orchardServices.WorkContext.CurrentSite.HomePage;
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