using System.Collections.Generic;

namespace Orchard.Localization {
    public interface ICultureManager : IDependency {
        IEnumerable<string> ListCultures();
        void AddCulture(string cultureName);
    }
}
