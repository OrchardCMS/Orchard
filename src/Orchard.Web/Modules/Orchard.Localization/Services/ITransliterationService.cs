using System.Collections.Generic;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public interface ITransliterationService : IDependency {
        string Convert(string value, int transliterationSpecificationId);
        IEnumerable<TransliterationSpecificationRecord> GetSpecifications();
    }
}