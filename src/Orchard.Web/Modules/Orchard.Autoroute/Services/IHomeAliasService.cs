using System.Web.Routing;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services {
    
    public interface IHomeAliasService : IDependency {
        RouteValueDictionary GetHomeRoute();
        int? GetHomePageId();
        bool IsHomePage(IContent content);
        void PublishHomeAlias(IContent content);
        void PublishHomeAlias(string route);
        void PublishHomeAlias(RouteValueDictionary route);
    }
}
