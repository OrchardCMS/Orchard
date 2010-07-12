using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common;
using Orchard.Core.Localization.Models;
using Orchard.Core.Localization.Services;
using Orchard.Core.Localization.ViewModels;
using Orchard.Localization.Services;

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

        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(Localized part, string displayType) {
            // for viewing or adding translation
            if (!Services.Authorizer.Authorize(Permissions.ChangeOwner)) {
                return null;
            }

            var model = new ContentLocalizationsViewModel(part) {
                CanLocalize = _cultureManager.ListCultures()
                    .Where(s => s != _cultureManager.GetCurrentCulture(new HttpContextWrapper(HttpContext.Current)) && s != _localizationService.GetContentCulture(part.ContentItem))
                    .Count() > 0,
                Localizations = _localizationService.GetLocalizations(part.ContentItem)
            };
            return ContentPartTemplate(model, "Parts/Localization.ContentTranslations").LongestMatch(displayType, "Summary", "SummaryAdmin").Location("primary", "5");
        }

        protected override DriverResult Editor(Localized part) {
            // ContentTranslations: for when there are drafts of translations
            // CultureSelection: for a new translation

            //var model = new SelectTranslationsViewModel(part);
            //if (part.ContentItem.Is<Localized>())

            return ContentPartTemplate(new SelectLocalizationsViewModel(part), "Parts/Localization.ContentTranslations").Location("secondary", "5");
        }
    }
}