using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public interface ILocalizationSetService : ILocalizationService, IDependency {
        IEnumerable<LocalizationPart> GetLocalizationSet(int localizationSetId, string contentType = "", VersionOptions versionOptions = null);
    }
}
