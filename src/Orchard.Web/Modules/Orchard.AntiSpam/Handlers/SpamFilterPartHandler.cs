using Orchard.AntiSpam.Models;

using Orchard.AntiSpam.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.AntiSpam.Handlers {
    public class SpamFilterPartHandler : ContentHandler {
        private readonly ITransactionManager _transactionManager;

        public SpamFilterPartHandler(
            IRepository<SpamFilterPartRecord> repository,
            ITransactionManager transactionManager
            ) {
            _transactionManager = transactionManager;
            Filters.Add(StorageFilter.For(repository));

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