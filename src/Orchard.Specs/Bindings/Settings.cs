using System;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents;
using Orchard.Data;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Specs.Hosting.Orchard.Web;
using TechTalk.SpecFlow;
using Orchard.Localization.Services;
using System.Linq;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class Settings : BindingBase {

        [When(@"I have ""(.*)"" as the default culture")]
        public void DefineDefaultCulture(string cultureName) {

            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using ( var environment = MvcApplication.CreateStandaloneEnvironment("Default") ) {
                    var orchardServices = environment.Resolve<IOrchardServices>();
                    var cultureManager = environment.Resolve<ICultureManager>();

                    var currentCultures = cultureManager.ListCultures();
                    if (!currentCultures.Contains(cultureName)) {
                        cultureManager.AddCulture(cultureName);
                    }

                    orchardServices.WorkContext.CurrentSite.SiteCulture = cultureName;
                }
            });
        }
    }
}
