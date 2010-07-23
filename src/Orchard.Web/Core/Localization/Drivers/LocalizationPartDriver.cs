using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Localization.Models;
using Orchard.Core.Localization.Services;
using Orchard.Core.Localization.ViewModels;
using Orchard.Localization.Services;

namespace Orchard.Core.Localization.Drivers {
    [UsedImplicitly]
    public class LocalizationPartDriver : ContentPartDriver<LocalizationPart> {
        private const string TemplatePrefix = "Localization";
        private readonly ICultureManager _cultureManager;
        private readonly ILocalizationService _localizationService;

        public LocalizationPartDriver(ICultureManager cultureManager, ILocalizationService localizationService) {
            _cultureManager = cultureManager;
            _localizationService = localizationService;
        }

        protected override DriverResult Display(LocalizationPart part, string displayType) {
            var model = new ContentLocalizationsViewModel(part) {
                Localizations = GetDisplayLocalizations(part)
            };

            return ContentPartTemplate(model, "Parts/Localization.ContentTranslations", TemplatePrefix).LongestMatch(displayType, "Summary", "SummaryAdmin").Location(part.GetLocation(displayType));
        }

        protected override DriverResult Editor(LocalizationPart part) {
            var localizations = GetEditorLocalizations(part).ToList();
            var model = new EditLocalizationViewModel {
                SelectedCulture = part.Culture != null ? part.Culture.Culture : null,
                SiteCultures = _cultureManager.ListCultures().Where(s => s != _cultureManager.GetSiteCulture() && !localizations.Select(l => l.Culture.Culture).Contains(s)),
                MasterContentItem = part.MasterContentItem,
                ContentLocalizations = new ContentLocalizationsViewModel(part) { Localizations = localizations }
            };

            return ContentPartTemplate(model, "Parts/Localization.Translation", TemplatePrefix).Location(part.GetLocation("Editor"));
        }

        protected override DriverResult Editor(LocalizationPart part, IUpdateModel updater) {
            var model = new EditLocalizationViewModel();
            if (updater != null && updater.TryUpdateModel(model, TemplatePrefix, null, null)) {
                _localizationService.SetContentCulture(part, model.SelectedCulture);
            }

            return Editor(part);
        }

        private IEnumerable<LocalizationPart> GetDisplayLocalizations(LocalizationPart part) {
            return _localizationService.GetLocalizations(part.ContentItem)
                .Select(c => {
                            var localized = c.ContentItem.As<LocalizationPart>();
                            if (localized.Culture == null) {
                                localized.Culture = _cultureManager.GetCultureByName(_cultureManager.GetCurrentCulture(new HttpContextWrapper(HttpContext.Current)));
                            }
                            return c;
                        }).ToList();
        }

        private IEnumerable<LocalizationPart> GetEditorLocalizations(LocalizationPart part) {
            return _localizationService.GetLocalizations(part.ContentItem)
                .Select(c => c.ContentItem.As<LocalizationPart>())
                .Where(l => l.MasterContentItem != null).ToList();
        }
    }
}