using System.Linq;
using Orchard.Localization.Services;
using Orchard.Specs.Hosting.Orchard.Web;
using TechTalk.SpecFlow;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class Settings : BindingBase {

        [When(@"I have ""(.*)"" as the default culture")]
        public void DefineDefaultCulture(string cultureName) {

            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    var orchardServices = environment.Resolve<IOrchardServices>();
                    var cultureManager = environment.Resolve<ICultureManager>();

                    var currentCultures = cultureManager.ListCultures();
                    if (!currentCultures.Contains(cultureName)) {
                        cultureManager.AddCulture(cultureName);
                    }

                    orchardServices.WorkContext.CurrentSite.SiteCulture = cultureName;

                    // Restarting the shell to reset the cache, because the cache entry storing the list of available
                    // cultures isn't invalidated by the signal in DefaultCultureManager.ListCultures when running
                    // inside the test webhost.
                    MvcApplication.RestartTenant("Default");
                }
            });
        }
    }
}
