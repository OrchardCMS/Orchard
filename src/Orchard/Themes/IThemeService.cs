using System;
using System.Web.Routing;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes {
    public interface IThemeService : IDependency {
        [Obsolete]
        ExtensionDescriptor GetRequestTheme(RequestContext requestContext);
    }
}
