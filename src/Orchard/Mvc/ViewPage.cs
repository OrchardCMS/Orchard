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
        public MvcHtmlString _Encoded(string textHint, params object[] formatTokens) {
            return MvcHtmlString.Create(Html.Encode(T(textHint, formatTokens)));
        }
    }

    public class ViewUserControl<TModel> : System.Web.Mvc.ViewUserControl<TModel> {
        public ViewUserControl() {
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
        public MvcHtmlString _Encoded(string textHint, params object[] formatTokens) {
            return MvcHtmlString.Create(Html.Encode(T(textHint, formatTokens)));
        }
    }
}
