using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.Localization;

namespace Orchard.Mvc {
    public class ViewPage<TModel> : System.Web.Mvc.ViewPage<TModel> {
        public MvcHtmlString T(string textHint) {
            return MvcHtmlString.Create(
                Html.Encode(new LocalizedString(textHint)));
        }
    }
}
