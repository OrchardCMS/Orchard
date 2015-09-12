using System.Web.Routing;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services {
    
    public interface IHomeAliasService : IDependency {
        RouteValueDictionary GetHomeRoute();
        int? GetHomePageId(VersionOptions version = null);
        IContent GetHomePage(VersionOptions version = null);
        bool IsHomePage(IContent content, VersionOptions homePageVersion = null);
        void PublishHomeAlias(IContent content);
        void PublishHomeAlias(string route);
        void PublishHomeAlias(RouteValueDictionary route);
    }
}
