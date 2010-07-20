using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Localization.Models;
using Orchard.Core.Localization.Services;
using Orchard.Core.Localization.ViewModels;
using Orchard.Localization.Services;

namespace Orchard.Core.Localization.Drivers {
    [UsedImplicitly]
    public class LocalizationDriver : ContentPartDriver<Localized> {
        private const string TemplatePrefix = "Localization";
        private readonly ICultureManager _cultureManager;
        private readonly ILocalizationService _localizationService;

        public LocalizationDriver(ICultureManager cultureManager, ILocalizationService localizationService) {
            _cultureManager = cultureManager;
            _localizationService = localizationService;
        }

        protected override DriverResult Display(Localized part, string displayType) {
            var model = new ContentLocalizationsViewModel(part) {
                Localizations = GetDisplayLocalizations(part)
            };

            return ContentPartTemplate(model, "Parts/Localization.ContentTranslations", TemplatePrefix).LongestMatch(displayType, "Summary", "SummaryAdmin").Location("primary", "5");
        }

        protected override DriverResult Editor(Localized part) {
            var localizations = GetEditorLocalizations(part).ToList();
            var model = new EditLocalizationViewModel {
                SelectedCulture = part.Culture != null ? part.Culture.Culture : null,
                SiteCultures = _cultureManager.ListCultures().Where(s => s != _cultureManager.GetSiteCulture() && !localizations.Select(l => l.Culture.Culture).Contains(s)),
                MasterContentItem = part.MasterContentItem,
                ContentLocalizations = new ContentLocalizationsViewModel(part) { Localizations = localizations }
            };

            return ContentPartTemplate(model, "Parts/Localization.Translation", TemplatePrefix).Location("primary", "1");
        }

        protected override DriverResult Editor(Localized part, IUpdateModel updater) {
            var model = new EditLocalizationViewModel();
            if (updater != null && updater.TryUpdateModel(model, TemplatePrefix, null, null)) {
                _localizationService.SetContentCulture(part, model.SelectedCulture);
            }

            return Editor(part);
        }

        private IEnumerable<Localized> GetDisplayLocalizations(Localized part) {
            return _localizationService.GetLocalizations(part.ContentItem)
                .Select(c => {
                            var localized = c.ContentItem.As<Localized>();
                            if (localized.Culture == null) {
                                localized.Culture = _cultureManager.GetCultureByName(_cultureManager.GetCurrentCulture(new HttpContextWrapper(HttpContext.Current)));
                            }
                            return c;
                        }).ToList();
        }

        private IEnumerable<Localized> GetEditorLocalizations(Localized part) {
            return _localizationService.GetLocalizations(part.ContentItem)
                .Select(c => c.ContentItem.As<Localized>())
                .Where(l => l.MasterContentItem != null).ToList();
        }
    }
}