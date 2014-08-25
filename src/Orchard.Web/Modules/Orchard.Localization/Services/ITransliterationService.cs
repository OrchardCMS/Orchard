using System.Collections.Generic;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public interface ITransliterationService : IDependency {
        string Convert(string value, string cultureFrom);
        IEnumerable<TransliterationSpecificationRecord> GetSpecifications();
        void Create(string cultureFrom, string cultureTo, string rules);
        void Update(int id, string cultureFrom, string cultureTo, string rules);
        void Remove(int id);
        TransliterationSpecificationRecord Get(int id);
    }
}