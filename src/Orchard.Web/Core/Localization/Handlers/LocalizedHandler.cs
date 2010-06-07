using JetBrains.Annotations;
using Orchard.Core.Localization.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Services;

namespace Orchard.Core.Localization.Handlers {
    [UsedImplicitly]
    public class LocalizedHandler : ContentHandler {
        private readonly ICultureManager _cultureManager;
        private readonly IContentManager _contentManager;

        public LocalizedHandler(IRepository<LocalizedRecord> localizedRepository, ICultureManager cultureManager, IContentManager contentManager) {
            _cultureManager = cultureManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(localizedRepository));

            OnActivated<Localized>(InitializePart);

            OnLoaded<Localized>(LazyLoadHandlers);
        }

        public Localizer T { get; set; }

        void LazyLoadHandlers(LoadContentContext context, Localized localized) {
            localized.CultureField.Loader(ctx => _cultureManager.GetCultureById(localized.Record.CultureId));
            localized.MasterContentItemField.Loader(ctx => _contentManager.Get(localized.Record.MasterContentItemId)); 
        }

        void InitializePart(ActivatedContentContext context, Localized localized) {
            localized.CultureField.Setter(cultureRecord => {
                localized.Record.CultureId = cultureRecord.Id;
                return cultureRecord;
            });
            localized.MasterContentItemField.Setter(masterContentItem => {
                localized.Record.MasterContentItemId = masterContentItem.ContentItem.Id;
                return masterContentItem;
            });
        }
    }
}