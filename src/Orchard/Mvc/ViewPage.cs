using System.Web.Mvc;
using Orchard.Localization;

namespace Orchard.Mvc {
    public class ViewPage<TModel> : System.Web.Mvc.ViewPage<TModel> {
        public ViewPage() {
            T = NullLocalizer.Instance;
        }

        public override void RenderView(ViewContext viewContext) {
            T = LocalizationUtilities.Resolve(viewContext, AppRelativeVirtualPath);
            base.RenderView(viewContext);
        }

        public Localizer T { get; set; }

        public MvcHtmlString H(string value) {
            return MvcHtmlString.Create(Html.Encode(value));
        }

        public MvcHtmlString _Encoded(string textHint) {
            return MvcHtmlString.Create(Html.Encode(T(textHint)));
        }
        public MvcHtmlString _Encoded(string textHint, params string[] formatTokens) {
            return MvcHtmlString.Create(Html.Encode(T(textHint, formatTokens)));
        }
    }
}
