using System.Linq;
using System.Web.Mvc;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ViewModels;
using Orchard.Utility.Extensions;

namespace Orchard.DynamicForms.Controllers {
    public class AdminController : Controller {
        private readonly IFormService _formService;
        public AdminController(IFormService formService) {
            _formService = formService;
        }

        public ActionResult Index() {
            var forms = _formService.GetSubmissions().ToArray().GroupBy(x => x.FormName).ToArray();
            var viewModel = new FormsIndexViewModel {
                Forms = forms
            };
            return View(viewModel);
        }

        public ActionResult GetTechnicalName(string displayName, int version) {
            return Json(new {
                result = displayName.ToHtmlName(),
                version = version
            });
        }
    }
}