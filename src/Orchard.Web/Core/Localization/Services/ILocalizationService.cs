using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Localization.Models;

namespace Orchard.Core.Localization.Services {
    public interface ILocalizationService : IDependency {
        LocalizationPart GetLocalizedContentItem(IContent masterContentItem, string culture);
        string GetContentCulture(IContent contentItem);
        void SetContentCulture(IContent contentItem, string culture);
        IEnumerable<LocalizationPart> GetLocalizations(IContent contentItem);
    }
}
