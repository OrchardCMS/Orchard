using System.Collections.Generic;
using System.Web;
using Orchard.Localization.Records;

namespace Orchard.Localization.Services {
    public interface ICultureManager : IDependency {
        IEnumerable<string> ListCultures();
        void AddCulture(string cultureName);
        void DeleteCulture(string cultureName);
        string GetCurrentCulture(HttpContext requestContext);
        CultureRecord GetCultureById(int id);
        string GetSiteCulture();
    }
}
