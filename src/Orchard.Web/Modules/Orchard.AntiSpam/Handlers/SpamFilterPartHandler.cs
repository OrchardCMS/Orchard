using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.AntiSpam.Handlers {
    public class SpamFilterPartHandler : ContentHandler {
        public SpamFilterPartHandler(ISpamService spamService, IRepository<SpamFilterPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnUpdated<SpamFilterPart>( (context, part) => {
                part.Status = spamService.CheckForSpam(part);
            });
        }
    }
}