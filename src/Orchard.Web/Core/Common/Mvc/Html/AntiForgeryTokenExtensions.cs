using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Mvc.Html;
using Orchard.Settings;

namespace Orchard.Core.Common.Mvc.Html {
    public static class AntiForgeryTokenExtensions {
        public static MvcHtmlString AntiForgeryTokenOrchard(this HtmlHelper htmlHelper)
        {
            var siteSalt = htmlHelper.Resolve<ISiteService>().GetSiteSettings().ContentItem.As<SiteSettings>().Record.SiteSalt;
            return htmlHelper.AntiForgeryToken(siteSalt);
        }
    }
}