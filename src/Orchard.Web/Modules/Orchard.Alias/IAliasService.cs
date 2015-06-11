using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.Alias {
    public interface IAliasService : IDependency {
        RouteValueDictionary Get(string aliasPath);
        void Set(string aliasPath, RouteValueDictionary routeValues, string aliasSource);
        void Set(string aliasPath, string routePath, string aliasSource);
        void Delete(string aliasPath);
        void Delete(string aliasPath, string aliasSource);
        /// <summary>
        /// Delete Alias from a particular source
        /// </summary>
        /// <param name="aliasSource"></param>
        void DeleteBySource(string aliasSource);

        IEnumerable<string> Lookup(RouteValueDictionary routeValues);
        IEnumerable<string> Lookup(string routePath);

        void Replace(string aliasPath, RouteValueDictionary routeValues, string aliasSource);
        void Replace(string aliasPath, string routePath, string aliasSource);

        IEnumerable<Tuple<string, RouteValueDictionary>> List();
        IEnumerable<Tuple<string, RouteValueDictionary, string>> List(string sourceStartsWith);
        IEnumerable<VirtualPathData> LookupVirtualPaths(RouteValueDictionary routeValues, System.Web.HttpContextBase HttpContext);

    }
}
