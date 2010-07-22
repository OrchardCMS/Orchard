using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.Handlers {
    [UsedImplicitly]
    public class SandboxContentHandler : ContentHandler {
        public SandboxContentHandler(IRepository<SandboxPageRecord> pageRepository, IRepository<SandboxSettingsRecord> settingsRepository) {
            // define the "SandboxPage" content type
            Filters.Add(StorageFilter.For(pageRepository) );

            // add settings to site, and simple record-template gui
            Filters.Add(new ActivatingFilter<ContentPart<SandboxSettingsRecord>>("Site"));
            Filters.Add(StorageFilter.For(settingsRepository));
            Filters.Add(new TemplateFilterForRecord<SandboxSettingsRecord>("SandboxSettings", "Parts/Sandbox.SiteSettings"));
        }
    }
}