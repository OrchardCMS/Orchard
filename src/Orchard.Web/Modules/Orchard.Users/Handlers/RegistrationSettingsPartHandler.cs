using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    [UsedImplicitly]
    public class RegistrationSettingsPartHandler : ContentHandler {
        public RegistrationSettingsPartHandler(IRepository<RegistrationSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<RegistrationSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<RegistrationSettingsPartRecord>("RegistrationSettings"));
        }
    }
}