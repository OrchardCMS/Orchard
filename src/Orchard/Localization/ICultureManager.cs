using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.Localization {
    public interface ICultureManager : IDependency {
        IEnumerable<string> ListCultures();
        void AddCulture(string cultureName);
        string GetCurrentCulture(RequestContext requestContext);
    }
}
