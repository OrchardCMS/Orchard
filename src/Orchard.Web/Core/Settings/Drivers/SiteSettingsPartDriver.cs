using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization.Services;
using Orchard.Settings;

namespace Orchard.Core.Settings.Drivers {
    [UsedImplicitly]
    public class SiteSettingsPartDriver : ContentPartDriver<SiteSettingsPart> {
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;

        public SiteSettingsPartDriver(ISiteService siteService, ICultureManager cultureManager) {
            _siteService = siteService;
            _cultureManager = cultureManager;
        }

        protected override string Prefix { get { return "SiteSettings"; } }

        protected override DriverResult Editor(SiteSettingsPart part, dynamic shapeHelper) {
            var site = _siteService.GetSiteSettings().As<SiteSettingsPart>();

            var model = new SiteSettingsPartViewModel {
                Site = site,
                SiteCultures = _cultureManager.ListCultures()
            };

            return ContentPartTemplate(model, "Parts/Settings.SiteSettingsPart");
        }

        protected override DriverResult Editor(SiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var site = _siteService.GetSiteSettings().As<SiteSettingsPart>();
            var model = new SiteSettingsPartViewModel { Site = site };

            if (updater.TryUpdateModel(model, Prefix, null, null)) {
            }

            return ContentPartTemplate(model, "Parts/Settings.SiteSettingsPart");
        }
    }
}