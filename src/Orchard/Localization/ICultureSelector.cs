using System.Web;

namespace Orchard.Localization {
    public class CultureSelectorResult {
        public int Priority { get; set; }
        public string CultureName { get; set; }
    }

    public interface ICultureSelector : IDependency {
        CultureSelectorResult GetCulture(HttpContext context);
    }
}
