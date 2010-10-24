using JetBrains.Annotations;
using Orchard.Email.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Email.Handlers {
    [UsedImplicitly]
    public class SmtpSettingsPartHandler : ContentHandler {
        public SmtpSettingsPartHandler(IRepository<SmtpSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<SmtpSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}