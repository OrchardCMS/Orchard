using System.Collections.Generic;
using Orchard.Core.Localization.Models;

namespace Orchard.Core.Localization.Services {
    public interface IContentItemLocalizationService : IDependency {
        IEnumerable<Localized> Get();
        Localized Get(int localizedId);
        Localized GetLocalizationForCulture(int masterId, string cultureName);
    }
}