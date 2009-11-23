using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;

namespace Orchard.Sandbox.Models {
    public class SandboxContentProvider : ContentProvider {
        public SandboxContentProvider(
            IRepository<SandboxPageRecord> pageRepository, 
            IRepository<SandboxSettingsRecord> settingsRepository) {

            // define the "sandboxpage" content type
            Filters.Add(new ActivatingFilter<SandboxPage>("sandboxpage"));
            Filters.Add(new ActivatingFilter<CommonPart>("sandboxpage"));
            Filters.Add(new ActivatingFilter<RoutablePart>("sandboxpage"));
            Filters.Add(new ActivatingFilter<BodyPart>("sandboxpage"));
            Filters.Add(new StorageFilter<SandboxPageRecord>(pageRepository) { AutomaticallyCreateMissingRecord = true });

            // add settings to site, and simple record-template gui
            Filters.Add(new ActivatingFilter<ContentPart<SandboxSettingsRecord>>("site"));
            Filters.Add(new StorageFilter<SandboxSettingsRecord>(settingsRepository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<SandboxSettingsRecord>("SandboxSettings"));

        }
    }
}
