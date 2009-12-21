using System.Collections.Generic;
using System.Web.Routing;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Sandbox.Models {
    public class SandboxContentHandler : ContentHandler {
        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] { SandboxPage.ContentType };
        }

        public SandboxContentHandler(
            IRepository<SandboxPageRecord> pageRepository,
            IRepository<SandboxSettingsRecord> settingsRepository) {

            // define the "sandboxpage" content type
            Filters.Add(new ActivatingFilter<SandboxPage>(SandboxPage.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(SandboxPage.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(SandboxPage.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(SandboxPage.ContentType.Name));
            Filters.Add(new StorageFilter<SandboxPageRecord>(pageRepository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new ContentItemTemplates<SandboxPage>("Items/Sandbox.Page", "Summary"));

            OnGetItemMetadata<SandboxPage>((context, page) => {
                context.Metadata.DisplayText = page.Record.Name;
                context.Metadata.DisplayRouteValues =
                    new RouteValueDictionary(
                        new {
                            area = "Orchard.Sandbox",
                            controller = "Page",
                            action = "Show",
                            id = context.ContentItem.Id,
                        });
                context.Metadata.EditorRouteValues =
                    new RouteValueDictionary(
                        new {
                            area = "Orchard.Sandbox",
                            controller = "Page",
                            action = "Edit",
                            id = context.ContentItem.Id,
                        });
            });

            OnGetDisplayViewModel<SandboxPage>((context, page) =>
                context.AddDisplay(new TemplateViewModel(page) { TemplateName = "ContentItemTitle", ZoneName = "title" }));



            // add settings to site, and simple record-template gui
            Filters.Add(new ActivatingFilter<ContentPart<SandboxSettingsRecord>>("site"));
            Filters.Add(new StorageFilter<SandboxSettingsRecord>(settingsRepository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<SandboxSettingsRecord>("SandboxSettings"));

        }
    }
}
