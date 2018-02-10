using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Themes;

namespace Orchard.Tokens.Controllers {
    public class AdminController : Controller {
        private readonly ITokenManager _tokenManager;

        public AdminController(ITokenManager tokenManager) {
            _tokenManager = tokenManager;
        }

        [Themed(false)]
        public ActionResult Tokens() {
            var tokenTypes = _tokenManager.Describe(Enumerable.Empty<string>());
            var results = new List<object>();

            foreach (var tokenType in tokenTypes.OrderBy(d => d.Name.ToString()))
            {
                results.Add(new {
                    label = tokenType.Name.Text,
                    desc = tokenType.Description.Text,
                    value = string.Empty
                });

                foreach(var token in tokenType.Tokens) {
                    results.Add(new {
                        label = token.Name.Text,
                        desc = token.Description.Text,
                        value = "{" + token.Target + "." + token.Token + "}"
                    });
                }
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}