using System.Web;

namespace Orchard.Localization.Services {
    public class CultureSelectorResult {
        public double Priority { get; set; }
        public string CultureName { get; set; }
    }

    public interface ICultureSelector : IDependency {
        CultureSelectorResult GetCulture(HttpContextBase context);
    }
}
