using System.Web.Routing;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services {
    
    public interface IHomeAliasService : IDependency {
        RouteValueDictionary GetHomeRoute();
        int? GetHomePageId();
        IContent GetHomePage(VersionOptions version = null);
        void PublishHomeAlias(IContent content);
        void PublishHomeAlias(string route);
        void PublishHomeAlias(RouteValueDictionary route);
    }
}
