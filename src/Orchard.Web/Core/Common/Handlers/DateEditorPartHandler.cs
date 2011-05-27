using System.Linq;
using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Services;

namespace Orchard.Core.Common.Handlers {
    [UsedImplicitly]
    public class DateEditorPartHandler : ContentHandler {
        private readonly IRepository<CommonPartVersionRecord> _commonPartVersionRepository;
        private readonly IClock _clock;

        public DateEditorPartHandler(
            IRepository<CommonPartVersionRecord> commonPartVersionRepository,
            IClock clock) {
            _commonPartVersionRepository = commonPartVersionRepository;
            _clock = clock;

            OnPublished<CommonPart>(AssignCreatingDates);

        }

        protected void AssignCreatingDates(PublishContentContext context, CommonPart part) {
            var commonEditorsSettings = CommonEditorsSettings.Get(part.ContentItem);
            if (!commonEditorsSettings.ShowDateEditor) {
                return;
            }

            // fetch CommonPartVersionRecord of first version
            var firstVersion = _commonPartVersionRepository.Fetch(
                civr => civr.ContentItemRecord == part.ContentItem.Record,
                order => order.Asc(record => record.ContentItemVersionRecord.Number),
                0, 1).FirstOrDefault();

            if (firstVersion != null && firstVersion.CreatedUtc == part.CreatedUtc) {
                // "touch" CreatedUtc in ContentItemRecord
                part.Record.CreatedUtc = _clock.UtcNow;
            }
        }
    }
}
