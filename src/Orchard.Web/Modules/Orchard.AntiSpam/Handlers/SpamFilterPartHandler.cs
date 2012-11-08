using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Services;
using Orchard.AntiSpam.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.AntiSpam.Handlers {
    public class SpamFilterPartHandler : ContentHandler {
        private readonly ITransactionManager _transactionManager;

        public SpamFilterPartHandler(
            ISpamService spamService, 
            IRepository<SpamFilterPartRecord> repository,
            ITransactionManager transactionManager
            ) {
            _transactionManager = transactionManager;
            Filters.Add(StorageFilter.For(repository));

            OnUpdated<SpamFilterPart>( (context, part) => {
                part.Status = spamService.CheckForSpam(part);
            });

            OnPublishing<SpamFilterPart>((context, part) => {
                if (part.Status == SpamStatus.Spam) {
                    if (part.Settings.GetModel<SpamFilterPartSettings>().DeleteSpam) {
                        _transactionManager.Cancel();
                    }

                    context.Cancel = true;
                }
            });

        }
    }
}