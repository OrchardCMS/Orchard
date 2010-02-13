using System;
using System.Collections.Generic;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Mvc.ViewModels;
using Orchard.Services;
using Orchard.Settings;

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

        public ActionResult Index() {
            try {
                string homepage = CurrentSite.HomePage;
                if (String.IsNullOrEmpty(homepage)) {
                    return View(new BaseViewModel());
                }

                string[] homePageParameters = homepage.Split(';');
                if (homePageParameters.Length != 2) {
                    return View(new BaseViewModel());
                }
                string providerName = homePageParameters[0];
                int item = Int32.Parse(homePageParameters[1]);

                foreach (var provider in _homePageProviders) {
                    if (String.Equals(provider.GetProviderName(), providerName)) {
                        ActionResult result = provider.GetHomePage(item);
                        if (result is ViewResultBase) {
                            ViewResultBase resultBase = result as ViewResultBase;
                            ViewData.Model = resultBase.ViewData.Model;
                            resultBase.ViewData = ViewData;
                        }
                        return result;
                    }
                }

                return View(new BaseViewModel());
            }
            catch {
                return View(new BaseViewModel());
            }
        }
    }

}