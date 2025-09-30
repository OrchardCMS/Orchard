using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public class LocalizationService : ILocalizationService {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;
        private readonly IHomeAliasService _homeAliasService;

        public LocalizationService(IContentManager contentManager, ICultureManager cultureManager, IHomeAliasService homeAliasService) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
            _homeAliasService = homeAliasService;
        }

        /// <summary>
        /// Warning: Returns only the first item of same culture localizations.
        /// </summary>
        public LocalizationPart GetLocalizedContentItem(IContent content, string culture) =>
            GetLocalizedContentItem(content, culture, null);

        /// <summary>
        /// Warning: Returns only the first item of same culture localizations.
        /// </summary>
        public LocalizationPart GetLocalizedContentItem(IContent content, string culture, VersionOptions versionOptions) {
            var cultureRecord = _cultureManager.GetCultureByName(culture);

            if (cultureRecord == null) {
                return null;
            }

            var localized = content.As<LocalizationPart>();

            if (localized == null) {
                return null;
            }

            if (localized.Culture?.Culture == culture) return localized;

            return GetLocalizationsQuery(localized, versionOptions)
                .Where<LocalizationPartRecord>(localization => localization.CultureId == cultureRecord.Id)
                .Slice(1)
                .FirstOrDefault();
        }

        public string GetContentCulture(IContent content) =>
            content.As<LocalizationPart>()?.Culture?.Culture ?? _cultureManager.GetSiteCulture();

        public void SetContentCulture(IContent content, string culture) {
            var localized = content.As<LocalizationPart>();

            if (localized == null) return;

            localized.Culture = _cultureManager.GetCultureByName(culture);
        }

        /// <summary>
        /// Warning: May contain more than one localization of the same culture.
        /// </summary>
        public IEnumerable<LocalizationPart> GetLocalizations(IContent content) => GetLocalizations(content, null);

        /// <summary>
        /// Warning: May contain more than one localization of the same culture.
        /// </summary>
        public IEnumerable<LocalizationPart> GetLocalizations(IContent content, VersionOptions versionOptions) {
            if (content.ContentItem.Id == 0) return Enumerable.Empty<LocalizationPart>();

            var localized = content.As<LocalizationPart>();

            return GetLocalizationsQuery(localized, versionOptions)
                .Where<LocalizationPartRecord>(localization => localization.Id != localized.Id) // Exclude the current content.
                .List();
        }

        public bool TryGetRouteForUrl(string url, out AutoroutePart route) {
            route = _contentManager.Query<AutoroutePart, AutoroutePartRecord>()
                .ForVersion(VersionOptions.Published)
                .Where(r => r.DisplayAlias == url)
                .List()
                .FirstOrDefault();

            route = route ?? _homeAliasService.GetHomePage(VersionOptions.Latest).As<AutoroutePart>();

            return route != null;
        }

        public bool TryFindLocalizedRoute(ContentItem routableContent, string cultureName, out AutoroutePart localizedRoute) {
            if (!routableContent.Parts.Any(p => p.Is<ILocalizableAspect>())) {
                localizedRoute = null;

                return false;
            }

            IEnumerable<LocalizationPart> localizations = GetLocalizations(routableContent, VersionOptions.Published);

            ILocalizableAspect localizationPart = null, siteCultureLocalizationPart = null;
            foreach (var localization in localizations) {
                if (localization.Culture.Culture.Equals(cultureName, StringComparison.InvariantCultureIgnoreCase)) {
                    localizationPart = localization;

                    break;
                }

                if (localization.Culture == null && siteCultureLocalizationPart == null) {
                    siteCultureLocalizationPart = localization;
                }
            }

            if (localizationPart == null) {
                localizationPart = siteCultureLocalizationPart;
            }

            localizedRoute = localizationPart?.As<AutoroutePart>();

            return localizedRoute != null;
        }

        /// <summary>
        /// Warning: May contain more than one localization of the same culture.
        /// </summary>
        private IContentQuery<LocalizationPart> GetLocalizationsQuery(LocalizationPart localizationPart, VersionOptions versionOptions) {
            var masterId = localizationPart.HasTranslationGroup
                ? localizationPart.Record.MasterContentItemId
                : localizationPart.Id;

            var query = _contentManager.Query<LocalizationPart>(localizationPart.ContentItem.ContentType);

            if (versionOptions == null) {
                query = query.ForVersion(versionOptions);
            }

            return query
                .Where<LocalizationPartRecord>(localization => localization.Id == masterId || localization.MasterContentItemId == masterId);
        }
    }
}
