using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Specs.Hosting.Orchard.Web;
using TechTalk.SpecFlow;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class OrchardSiteFactory : BindingBase {
        [Given(@"I have a clean site with standard extensions")]
        public void GivenIHaveACleanSiteWithStandardExtensions() {
            GivenIHaveACleanSiteWithStandardExtensions("/");
        }

        [Given(@"I have a clean site with standard extensions at ""(.*)\""")]
        public void GivenIHaveACleanSiteWithStandardExtensions(string virtualDirectory) {
            Binding<WebAppHosting>().GivenIHaveACleanSiteWith(
                virtualDirectory,
                // This is the list of extensions which will be copied over into the temporary Orchard folder.
                TableData(
                    new { extension = "Module", names = "Lucene, Markdown, Orchard.Alias, Orchard.AntiSpam, Orchard.ArchiveLater, Orchard.Autoroute, Orchard.Azure, Orchard.Blogs, Orchard.Caching, Orchard.CodeGeneration, Orchard.Comments, Orchard.ContentPermissions, Orchard.ContentPicker, Orchard.ContentTypes, Orchard.DesignerTools, Orchard.Email, Orchard.Fields, Orchard.Forms, Orchard.ImageEditor, Orchard.ImportExport, Orchard.Indexing, Orchard.JobsQueue, Orchard.Resources, Orchard.Layouts, Orchard.Lists, Orchard.Localization, Orchard.MediaLibrary, Orchard.MediaProcessing, Orchard.Migrations, Orchard.Modules, Orchard.MultiTenancy, Orchard.OutputCache, Orchard.Packaging, Orchard.Pages, Orchard.Projections, Orchard.PublishLater, Orchard.Recipes, Orchard.Roles, Orchard.Scripting, Orchard.Scripting.CSharp, Orchard.Scripting.Dlr, Orchard.Search, Orchard.SecureSocketsLayer, Orchard.Setup, Orchard.Tags, Orchard.Taxonomies, Orchard.Templates, Orchard.Themes, Orchard.Tokens, Orchard.Users, Orchard.Warmup, Orchard.Widgets, Orchard.Workflows, Orchard.Conditions, SysCache, TinyMce, Upgrade" },
                    new { extension = "Core", names = "Common, Containers, Contents, Dashboard, Feeds, Navigation, Scheduling, Settings, Shapes, Title, XmlRpc" },
                    new { extension = "Theme", names = "SafeMode, TheAdmin, TheThemeMachine" }));
        }

        [Given(@"I have installed Orchard")]
        public void GivenIHaveInstalledOrchard() {
            GivenIHaveInstalledOrchard("/");
        }

        [Given(@"I have installed Orchard at ""(.*)\""")]
        public void GivenIHaveInstalledOrchard(string virtualDirectory) {
            var webApp = Binding<WebAppHosting>();

            GivenIHaveACleanSiteWithStandardExtensions(virtualDirectory);

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
            webApp.Host.Execute(MvcApplication.ReloadExtensions);

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

                // this is needed to force the tenant to restart when a new feature is enabled,
                // as DefaultOrchardHost maintains this list in a thread context otherwise
                // and looses the information
                MvcApplication.RestartTenant("Default");
            });

        }

        [Given(@"I have a containable content type ""(.*)\""")]
        public void GivenIHaveAContainableContentType(string name) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    var cdm = environment.Resolve<IContentDefinitionManager>();

                    var contentTypeDefinition = new ContentTypeDefinition(name, name);
                    cdm.StoreTypeDefinition(contentTypeDefinition);
                    cdm.AlterTypeDefinition(name, cfg => cfg.WithPart("CommonPart").WithPart("BodyPart").WithPart("TitlePart").WithPart("ContainablePart").Creatable().Draftable());

                    cdm.AlterTypeDefinition(name,
                        cfg => cfg.WithPart("AutoroutePart",
                            builder => builder
                                .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                                .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                                .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-list'}]")
                                .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                        ));

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
                    State = TenantState.Uninitialized,
                };
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    environment.Resolve<IShellSettingsManager>().SaveSettings(shellSettings);
                }

                MvcApplication.RestartTenant(shellName);
            });

            webApp.WhenIGoToPathOnHost("Setup", hostName);

            webApp.WhenIFillIn(TableData(
                new { name = "SiteName", value = siteName },
                new { name = "AdminPassword", value = "6655321" }));

            webApp.WhenIHit("Finish Setup");
        }
    }
}
