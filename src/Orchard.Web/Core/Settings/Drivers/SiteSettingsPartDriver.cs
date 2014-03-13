using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Settings;
using System;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Localization;

namespace Orchard.Core.Settings.Drivers {
    [UsedImplicitly]
    public class SiteSettingsPartDriver : ContentPartDriver<SiteSettingsPart> {
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;
		private readonly ICalendarManager _calendarProvider;
        private readonly IMembershipService _membershipService;
        private readonly INotifier _notifier;
        private readonly IAuthorizer _authorizer;

        public SiteSettingsPartDriver(
            ISiteService siteService, 
            ICultureManager cultureManager,
			ICalendarManager calendarProvider,
            IMembershipService membershipService, 
            INotifier notifier,
            IAuthorizer authorizer) {
            _siteService = siteService;
            _cultureManager = cultureManager;
			_calendarProvider = calendarProvider;
            _membershipService = membershipService;
            _notifier = notifier;
            _authorizer = authorizer;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override string Prefix { get { return "SiteSettings"; } }

        protected override DriverResult Editor(SiteSettingsPart part, dynamic shapeHelper) {
            var site = _siteService.GetSiteSettings().As<SiteSettingsPart>();

            var model = new SiteSettingsPartViewModel {
                Site = site,
                SiteCultures = _cultureManager.ListCultures(),
				SiteCalendars = _calendarProvider.ListCalendars(),
                TimeZones = TimeZoneInfo.GetSystemTimeZones()
            };

            return ContentShape("Parts_Settings_SiteSettingsPart",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Settings.SiteSettingsPart", Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(SiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var site = _siteService.GetSiteSettings().As<SiteSettingsPart>();
            var model = new SiteSettingsPartViewModel { 
                Site = site,
                SiteCultures = _cultureManager.ListCultures(),
				SiteCalendars = _calendarProvider.ListCalendars(),
				TimeZones = TimeZoneInfo.GetSystemTimeZones()
            };

            var previousBaseUrl = model.Site.BaseUrl;
            var previousSuperUser = model.Site.SuperUser;
            var previousMaxPageSize = model.Site.MaxPageSize;

            updater.TryUpdateModel(model, Prefix, null, null);

            // only a user with SiteOwner permission can change the site owner
            if (_authorizer.Authorize(StandardPermissions.SiteOwner)) {
                // ensures the super user is fully empty
                if (String.IsNullOrEmpty(model.SuperUser)) {
                    model.SuperUser = String.Empty;
                }
                    // otherwise the super user must be a valid user, to prevent an external account to impersonate as this name
                    //the user management module ensures the super user can't be deleted, but it can be disabled
                else {
                    if (_membershipService.GetUser(model.SuperUser) == null) {
                        updater.AddModelError("SuperUser", T("The user {0} was not found", model.SuperUser));
                    }
                }
            }
            else {
                model.Site.SuperUser = previousSuperUser;
                model.Site.MaxPageSize = previousMaxPageSize;
            }

            // ensure the base url is absolute if provided
            if (!String.IsNullOrWhiteSpace(model.Site.BaseUrl)) {
                if (!Uri.IsWellFormedUriString(model.Site.BaseUrl, UriKind.Absolute)) {
                    updater.AddModelError("BaseUrl", T("The base url must be absolute."));
                }
                    // if the base url has been modified, try to ping it
                else if (!String.Equals(previousBaseUrl, model.Site.BaseUrl, StringComparison.OrdinalIgnoreCase)) {
                    try {
                        var request = WebRequest.Create(model.Site.BaseUrl) as HttpWebRequest;
                        if (request != null) {
                            using (request.GetResponse() as HttpWebResponse) {}
                        }
                    }
                    catch (Exception e) {
                        _notifier.Warning(T("The base url you entered could not be requested from current location."));
                        Logger.Warning(e, "Could not query base url: {0}", model.Site.BaseUrl);
                    }
                }
            }

            return ContentShape("Parts_Settings_SiteSettingsPart",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Settings.SiteSettingsPart", Model: model, Prefix: Prefix));
        }
    }
}