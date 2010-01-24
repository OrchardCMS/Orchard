using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Sandbox.Controllers;

namespace Orchard.Sandbox.Models {
    [UsedImplicitly]
    public class SandboxContentHandler : ContentHandler {

        public SandboxContentHandler(
            IRepository<SandboxPageRecord> pageRepository,
            IRepository<SandboxSettingsRecord> settingsRepository) {

            // define the "sandboxpage" content type
            Filters.Add(new ActivatingFilter<SandboxPage>(SandboxPageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(SandboxPageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(SandboxPageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(SandboxPageDriver.ContentType.Name));
            Filters.Add(StorageFilter.For(pageRepository) );



            // add settings to site, and simple record-template gui
            Filters.Add(new ActivatingFilter<ContentPart<SandboxSettingsRecord>>("site"));
            Filters.Add(StorageFilter.For(settingsRepository));
            Filters.Add(new TemplateFilterForRecord<SandboxSettingsRecord>("SandboxSettings", "Parts/Sandbox.SiteSettings"));

        }
    }
}
