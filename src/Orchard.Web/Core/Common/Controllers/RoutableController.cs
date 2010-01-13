using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Orchard.Core.Common.Controllers {
    public class RoutableController : Controller {
        [HttpPost]
        public ActionResult Slugify(FormCollection formCollection, string value) {
            if (!string.IsNullOrEmpty(value)) {

                //todo: (heskew) improve - just doing multi-pass regex replaces for now with the simple rules of
                // (1) can't begin with a '/', (2) can't have adjacent '/'s and (3) can't have these characters
                var startsoffbad = new Regex(@"^[\s/]+");
                var slashhappy = new Regex("/{2,}");
                var dissallowed = new Regex(@"[:?#\[\]@!$&'()*+,;=\s]+");

                value = value.Trim();
                value = startsoffbad.Replace(value, "-");
                value = slashhappy.Replace(value, "/");
                value = dissallowed.Replace(value, "-");

                if (value.Length > 1000)
                    value = value.Substring(0, 1000);
            }

            return Json(value);
        }
    }
}