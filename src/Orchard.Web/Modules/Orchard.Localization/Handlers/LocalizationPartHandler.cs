using System.Globalization;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Models;
using Orchard.Localization.Services;

namespace Orchard.Localization.Handlers {
    public class LocalizationPartHandler : ContentHandler {
        private readonly ICultureManager _cultureManager;
        private readonly IContentManager _contentManager;

        public LocalizationPartHandler(IRepository<LocalizationPartRecord> localizedRepository, ICultureManager cultureManager, IContentManager contentManager) {
            _cultureManager = cultureManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(localizedRepository));

            OnActivated<LocalizationPart>(PropertySetHandlers);

            OnLoading<LocalizationPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<LocalizationPart>((context, part, versionedPart) => LazyLoadHandlers(versionedPart));

            OnIndexed<LocalizationPart>((context, localized) => context.DocumentIndex
                .Add("culture", CultureInfo.GetCultureInfo(localized.Culture != null ? localized.Culture.Culture : _cultureManager.GetSiteCulture()).LCID).Store()
                );
        }

        public Localizer T { get; set; }

        protected static void PropertySetHandlers(ActivatedContentContext context, LocalizationPart localizationPart) {
            localizationPart.CultureField.Setter(cultureRecord => {
                localizationPart.Store<LocalizationPart, LocalizationPartRecord, int>(r => r.CultureId,
                    cultureRecord != null ? cultureRecord.Id : 0);
                return cultureRecord;
            });

            localizationPart.MasterContentItemField.Setter(masterContentItem => {
                localizationPart.Store<LocalizationPart, LocalizationPartRecord, int>(r => r.MasterContentItemId,
                    masterContentItem.ContentItem.Id);
                return masterContentItem;
            });
        }

        protected void LazyLoadHandlers(LocalizationPart localizationPart) {
            localizationPart.CultureField.Loader(() =>
                _cultureManager.GetCultureById(
                    localizationPart.Retrieve<LocalizationPart, LocalizationPartRecord, int>(r => r.CultureId)));

            localizationPart.MasterContentItemField.Loader(() =>
                _contentManager.Get(
                    localizationPart.Retrieve<LocalizationPart, LocalizationPartRecord, int>(r => r.MasterContentItemId), VersionOptions.AllVersions));
        }
    }
}
