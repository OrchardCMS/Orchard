using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Localization.Models;

namespace Orchard.Core.Localization.Services {
    public interface ILocalizationService : IDependency {
        Localized GetLocalizedContentItem(IContent masterContentItem, string culture);
        string GetContentCulture(IContent contentItem);
        IEnumerable<Localized> GetLocalizations(IContent contentItem);
    }
}
