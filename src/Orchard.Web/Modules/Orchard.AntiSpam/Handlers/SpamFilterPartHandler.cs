using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Services;
using Orchard.AntiSpam.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.AntiSpam.Handlers {
    public class SpamFilterPartHandler : ContentHandler {
        private readonly ITransactionManager _transactionManager;
        private readonly ISpamService _spamService;

        public SpamFilterPartHandler(
            IRepository<SpamFilterPartRecord> repository,
            ITransactionManager transactionManager,
            ISpamService spamService
            ) {
            _transactionManager = transactionManager;
            _spamService = spamService;

            Filters.Add(StorageFilter.For(repository));

            OnCreating<SpamFilterPart>((context, part) => {
                part.Status = _spamService.CheckForSpam(part);
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