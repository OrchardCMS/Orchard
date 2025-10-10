using System.Collections.Generic;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public interface ILocalizationService : IDependency {
        LocalizationPart GetLocalizedContentItem(IContent content, string culture);
        LocalizationPart GetLocalizedContentItem(IContent content, string culture, VersionOptions versionOptions);
        string GetContentCulture(IContent content);
        void SetContentCulture(IContent content, string culture);
        IEnumerable<LocalizationPart> GetLocalizations(IContent content);
        IEnumerable<LocalizationPart> GetLocalizations(IContent content, VersionOptions versionOptions);
        bool TryFindLocalizedRoute(ContentItem routableContent, string cultureName, out AutoroutePart localizedRoute);
        bool TryGetRouteForUrl(string url, out AutoroutePart route);
    }
}
