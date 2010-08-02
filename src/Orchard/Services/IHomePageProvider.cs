using System.Web.Mvc;

namespace Orchard.Services {
    public interface IHomePageProvider : IDependency {
        string GetProviderName();
        string GetSettingValue(int itemId);
        ActionResult GetHomePage(int itemId);
    }
}
