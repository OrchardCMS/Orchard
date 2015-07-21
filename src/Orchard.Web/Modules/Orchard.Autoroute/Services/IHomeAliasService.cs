using System.Web.Routing;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services {
    
    public interface IHomeAliasService : IDependency {
        RouteValueDictionary GetHomeRoute();
        int? GetHomePageId();
        IContent GetHomePage(VersionOptions version = null);
        void SetHomeAlias(IContent content);
        void SetHomeAlias(string route);
        void SetHomeAlias(RouteValueDictionary route);
    }
}
