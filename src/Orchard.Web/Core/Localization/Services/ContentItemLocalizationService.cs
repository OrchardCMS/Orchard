using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Localization.Models;
using Orchard.Localization.Services;
using Localized = Orchard.Core.Localization.Models.Localized;

namespace Orchard.Core.Localization.Services {
    [UsedImplicitly]
    public class ContentItemLocalizationService : IContentItemLocalizationService {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;

        public ContentItemLocalizationService(IContentManager contentManager, ICultureManager cultureManager) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
        }

        public IEnumerable<Localized> Get() {
            return _contentManager.Query<Localized, LocalizedRecord>().List();
        }

        public Localized Get(int localizedId) {
            return _contentManager.Get<Localized>(localizedId);
        }

        public Localized GetLocalizationForCulture(int masterId, string cultureName) {
            var cultures = _cultureManager.ListCultures();
            if (cultures.Contains(cultureName)) {
                int cultureId = _cultureManager.GetCultureIdByName(cultureName);
                if (cultureId != 0) {
                    return _contentManager.Query<Localized, LocalizedRecord>()
                        .Where(x => x.MasterContentItemId == masterId && x.CultureId == cultureId).List().FirstOrDefault();
                }
            }
            return null;
        }
    }
}