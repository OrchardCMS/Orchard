using System.Web.Mvc;

namespace Orchard.Services {
    public interface IHomePageProvider : IDependency {
        string GetProviderName();
        ActionResult GetHomePage(int itemId);
    }
}
