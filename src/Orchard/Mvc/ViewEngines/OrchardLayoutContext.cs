using System.Web;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines {
    public class OrchardLayoutContext {
        private static readonly object _key = typeof(OrchardLayoutContext);

        public string BodyContent { get; set; }

        public static OrchardLayoutContext From(ControllerContext context) {
            return From(context.HttpContext);
        }

        public static OrchardLayoutContext From(HttpContextBase context) {
            if (!context.Items.Contains(_key)) {
                context.Items.Add(_key, new OrchardLayoutContext());
            }
            return (OrchardLayoutContext)context.Items[_key];
        }
    }
}
