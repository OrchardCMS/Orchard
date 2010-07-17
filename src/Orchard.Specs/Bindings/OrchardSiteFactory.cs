using System.Diagnostics;
using System.Linq;
using System.Text;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Specs.Hosting.Orchard.Web;
using TechTalk.SpecFlow;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class OrchardSiteFactory : BindingBase {
        [Given(@"I have installed Orchard")]
        public void GivenIHaveInstalledOrchard() {

            var webApp = Binding<WebAppHosting>();

            webApp.GivenIHaveACleanSiteWith(TableData(
                new { extension = "module", names = "Orchard.Setup, Orchard.Modules, Orchard.Themes, Orchard.Users, Orchard.Roles, Orchard.Comments, Orchard.Tags, TinyMce" },
                new { extension = "core", names = "Common, Dashboard, Feeds, HomePage, Navigation, Contents, PublishLater, Routable, Scheduling, Settings, XmlRpc" },
                new { extension = "theme", names = "SafeMode, Classic" }));

            webApp.WhenIGoTo("Setup");

            webApp.WhenIFillIn(TableData(
                new { name = "SiteName", value = "My Site" },
                new { name = "AdminPassword", value = "6655321" },
                new { name = "ConfirmPassword", value = "6655321" }));

            webApp.WhenIHit("Finish Setup");
        }

        [Given(@"I have installed ""(.*)\""")]
        public void GivenIHaveInstalled(string name) {
            var webApp = Binding<WebAppHosting>();
            webApp.GivenIHaveModule(name);
            webApp.Host.Execute(() => {
                MvcApplication.ReloadExtensions();
            });

            GivenIHaveEnabled(name);
        }

        [Given(@"I have enabled ""(.*)\""")]
        public void GivenIHaveEnabled(string name) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    var descriptorManager = environment.Resolve<IShellDescriptorManager>();
                    var descriptor = descriptorManager.GetShellDescriptor();
                    descriptorManager.UpdateShellDescriptor(
                        descriptor.SerialNumber,
                        descriptor.Features.Concat(new[] { new ShellFeature { Name = name } }),
                        descriptor.Parameters);
                }
            });

        }


        [Given(@"I have tenant ""(.*)\"" on ""(.*)\"" as ""(.*)\""")]
        public void GivenIHaveTenantOnSiteAsName(string shellName, string hostName, string siteName) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                var shellSettings = new ShellSettings {
                    Name = shellName,
                    RequestUrlHost = hostName,
                    State = new TenantState("Uninitialized"),
                };
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    environment.Resolve<IShellSettingsManager>().SaveSettings(shellSettings);
                }
            });

            webApp.WhenIGoToPathOnHost("Setup", hostName);

            webApp.WhenIFillIn(TableData(
                new { name = "SiteName", value = siteName },
                new { name = "AdminPassword", value = "6655321" }));

            webApp.WhenIHit("Finish Setup");
        }
    }
}
