using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common;
using Orchard.Core.Localization.Models;
using Orchard.Core.Localization.Services;
using Orchard.Core.Localization.ViewModels;
using Orchard.Localization.Services;
using Orchard.Settings;

namespace Orchard.Core.Localization.Drivers {
    [UsedImplicitly]
    public class LocalizationDriver : ContentPartDriver<Localized> {
        private readonly ICultureManager _cultureManager;
        private readonly ILocalizationService _localizationService;

        public LocalizationDriver(IOrchardServices services, ICultureManager cultureManager, ILocalizationService localizationService) {
            _cultureManager = cultureManager;
            _localizationService = localizationService;
            Services = services;
        }

        protected virtual ISite CurrentSite { get; private set; }
        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(Localized part, string displayType) {
            var model = new ContentLocalizationsViewModel(part) {
                Localizations = _localizationService.GetLocalizations(part.ContentItem)
                    .Select(c => {
                                var localized = c.ContentItem.As<Localized>();
                                if (localized.Culture == null)
                                    localized.Culture = _cultureManager.GetCultureByName(_cultureManager.GetCurrentCulture(new HttpContextWrapper(HttpContext.Current)));
                                return c;
                            }).ToList()
            };
            
            return ContentPartTemplate(model, "Parts/Localization.ContentTranslations").LongestMatch(displayType, "Summary", "SummaryAdmin").Location("primary", "5");
        }
    }
}