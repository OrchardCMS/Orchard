using System.Collections.Generic;
using System.Web;

namespace Orchard.Localization {
    public interface ICultureManager : IDependency {
        IEnumerable<string> ListCultures();
        void AddCulture(string cultureName);
        string GetCurrentCulture(HttpContext requestContext);
    }
}
