using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Mvc {
    public class ViewUserControl<TModel> : System.Web.Mvc.ViewUserControl<TModel> {
        public ViewUserControl() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override void RenderView(ViewContext viewContext) {
            T = LocalizationUtilities.Resolve(viewContext, AppRelativeVirtualPath);
            base.RenderView(viewContext);
        }

        public MvcHtmlString H(string value) {
            return MvcHtmlString.Create(Html.Encode(value));
        }

        public bool AuthorizedFor(Permission permission) {
            return Html.Resolve<IAuthorizer>().Authorize(permission);
        }
    }
}