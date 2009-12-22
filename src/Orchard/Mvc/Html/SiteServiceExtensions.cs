using System.Web.Mvc;
using Orchard.Settings;

namespace Orchard.Mvc.Html {
    public static class SiteServiceExtensions {
        public static string SiteName(this HtmlHelper html) {
            return html.Resolve<ISiteService>().GetSiteSettings().SiteName;
        }
    }
}