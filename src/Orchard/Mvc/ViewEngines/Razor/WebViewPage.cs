using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Mvc.ViewEngines.Razor {
    public abstract class WebViewPage : WebViewPage<object> { 
    }

    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel> {
        public WebViewPage() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override void InitHelpers() {
            base.InitHelpers();
            T = LocalizationUtilities.Resolve(ViewContext, VirtualPath);
        }

        public MvcHtmlString H(string value) {
            return MvcHtmlString.Create(Html.Encode(value));
        }

        public bool AuthorizedFor(Permission permission) {
            return Html.Resolve<IAuthorizer>().Authorize(permission);
        }
    }
    
}
