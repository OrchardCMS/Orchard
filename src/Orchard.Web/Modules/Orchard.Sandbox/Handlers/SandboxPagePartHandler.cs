using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.Handlers {
    [UsedImplicitly]
    public class SandboxPagePartHandler : ContentHandler {
        public SandboxPagePartHandler(IRepository<SandboxPagePartRecord> pageRepository, IRepository<SandboxSettingsPartRecord> settingsRepository) {
            // define the "SandboxPage" content type
            Filters.Add(StorageFilter.For(pageRepository) );

            // add settings to site, and simple record-template gui
            Filters.Add(new ActivatingFilter<ContentPart<SandboxSettingsPartRecord>>("Site"));
            Filters.Add(StorageFilter.For(settingsRepository));
            Filters.Add(new TemplateFilterForRecord<SandboxSettingsPartRecord>("SandboxSettings", "Parts/Sandbox.SiteSettings"));
        }
    }
}