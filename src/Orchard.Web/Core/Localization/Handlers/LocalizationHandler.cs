using System.Globalization;
using JetBrains.Annotations;
using Orchard.Core.Localization.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Services;

namespace Orchard.Core.Localization.Handlers {
    [UsedImplicitly]
    public class LocalizationHandler : ContentHandler {
        private readonly ICultureManager _cultureManager;
        private readonly IContentManager _contentManager;

        public LocalizationHandler(IRepository<LocalizedRecord> localizedRepository, ICultureManager cultureManager, IContentManager contentManager) {
            _cultureManager = cultureManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(localizedRepository));

            OnInitializing<Localized>(InitializePart);

            OnLoaded<Localized>(LazyLoadHandlers);

            OnIndexed<Localized>((context, localized) => context.DocumentIndex
                .Add("culture", CultureInfo.GetCultureInfo(localized.Culture != null ? localized.Culture.Culture : _cultureManager.GetSiteCulture()).LCID)
                .Store()
                );
        }

        public Localizer T { get; set; }

        void LazyLoadHandlers(LoadContentContext context, Localized localized) {
            localized.CultureField.Loader(ctx => _cultureManager.GetCultureById(localized.Record.CultureId));
            localized.MasterContentItemField.Loader(ctx => _contentManager.Get(localized.Record.MasterContentItemId)); 
        }

        void InitializePart(InitializingContentContext context, Localized localized) {
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