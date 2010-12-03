using System.Web.Mvc;

namespace Orchard.Services {
    public interface IHomePageProvider : IDependency {
        string GetProviderName();
        string GetSettingValue(int itemId);
        int GetHomePageId(string value);
        ActionResult GetHomePage(int itemId);
    }
}
