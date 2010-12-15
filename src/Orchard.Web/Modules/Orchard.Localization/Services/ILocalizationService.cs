using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public interface ILocalizationService : IDependency {
        LocalizationPart GetLocalizedContentItem(IContent masterContentItem, string culture);
        string GetContentCulture(IContent contentItem);
        void SetContentCulture(IContent contentItem, string culture);
        IEnumerable<LocalizationPart> GetLocalizations(IContent contentItem, VersionOptions versionOptions);
    }
}
