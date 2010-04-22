using System.Linq;
using System.Text;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Specs.Hosting.Orchard.Web;
using TechTalk.SpecFlow;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class OrchardSiteFactory : BindingBase {
        [Given(@"I have installed Orchard")]
        public void GivenIHaveInstalledOrchard() {

            var webApp = Binding<WebAppHosting>();

            webApp.GivenIHaveACleanSiteWith(TableData(
                new { extension = "module", names = "Orchard.Setup, Orchard.Users, Orchard.Roles, Orchard.Pages, Orchard.Comments, TinyMce" },
                new { extension = "core", names = "Common, Dashboard, Feeds, HomePage, Navigation, Scheduling, Settings, Themes, XmlRpc" },
                new { extension = "theme", names = "SafeMode, Classic" }));

            webApp.WhenIGoTo("Setup");

            webApp.WhenIFillIn(TableData(
                new { name = "SiteName", value = "My Site" },
                new { name = "AdminPassword", value = "6655321" }));

            webApp.WhenIHit("Finish Setup");
        }

        [Given(@"I have installed ""(.*)\""")]
        public void GivenIHaveInstalled(string name) {
            Binding<WebAppHosting>().GivenIHaveModule(name);
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
                        descriptor.EnabledFeatures.Concat(new[] { new ShellFeature { Name = name } }),
                        descriptor.Parameters);
                }
                MvcApplication.Host.Reinitialize_Obsolete();
            });

        }
    }
}
