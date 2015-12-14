using System.Web.Routing;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes {
    public interface IThemeManager : IDependency {
        ExtensionDescriptor GetRequestTheme(RequestContext requestContext);
    }
}
