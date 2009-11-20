using System.Web.Mvc;
using Orchard.Localization;

namespace Orchard.Mvc {
    public class ViewPage<TModel> : System.Web.Mvc.ViewPage<TModel> {
        public string _(string textHint)
        {
            return new LocalizedString(textHint).ToString();
        }
        public string _(string textHint, params string[] formatTokens)
        {
            return string.Format(_(textHint), formatTokens);
        }
        public MvcHtmlString _Encoded(string textHint)
        {
            return MvcHtmlString.Create(Html.Encode(_(textHint)));
        }
        public MvcHtmlString _Encoded(string textHint, params string[] formatTokens) {
            return MvcHtmlString.Create(Html.Encode(_(textHint, formatTokens)));
        }
    }
}
