using System;
using System.Net;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;
using Orchard.Exceptions;

namespace Orchard.Core.Settings.Drivers {
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

            // Update all properties but not SuperUser, MaxPageSize and BaseUrl.
            updater.TryUpdateModel(model, Prefix, null, new [] { "Site.SuperUser", "Site.MaxPageSize", "Site.BaseUrl", "Site.MaxPagedCount" });

            // only a user with SiteOwner permission can change the site owner
            if (_authorizer.Authorize(StandardPermissions.SiteOwner)) {
                updater.TryUpdateModel(model, Prefix, new[] { "Site.SuperUser", "Site.MaxPageSize", "Site.BaseUrl", "Site.MaxPagedCount" }, null);

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
                                using (request.GetResponse() as HttpWebResponse) { }
                            }
                        }
                        catch (Exception ex) {
                            if (ex.IsFatal()) {
                                throw;
                            }
                            _notifier.Warning(T("The base url you entered could not be requested from current location."));
                            Logger.Warning(ex, "Could not query base url: {0}", model.Site.BaseUrl);
                        }
                    }
                }
            }            

            return ContentShape("Parts_Settings_SiteSettingsPart",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Settings.SiteSettingsPart", Model: model, Prefix: Prefix));
        }

        protected override void Exporting(SiteSettingsPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("SupportedCultures", string.Join(";", _cultureManager.ListCultures()));
        }

        protected override void Importing(SiteSettingsPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "SupportedCultures", supportedCultures => {
                foreach (var culture in supportedCultures.Split(';')) {
                    _cultureManager.AddCulture(culture);
                }
            });
        }
    }
}