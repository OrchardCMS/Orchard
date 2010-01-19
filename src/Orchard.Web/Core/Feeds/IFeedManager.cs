using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.Core.Feeds {
    public interface IFeedManager : IDependency {
        void Register(string title, string format, RouteValueDictionary values);
        MvcHtmlString GetRegisteredLinks(HtmlHelper html);
        
        // Currently implemented in FeedController action... tbd
        //ActionResult Execute(string format, IValueProvider values);
    }
}
