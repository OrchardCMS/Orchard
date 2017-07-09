using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ViewModels;
using Orchard.Settings;
using Orchard.UI.Navigation;

namespace Orchard.DynamicForms.Controllers {
    public class AdminController : Controller {
        private readonly ISiteService _siteService;
        private readonly IFormService _formService;
        public AdminController(IFormService formService,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            _formService = formService;
            _siteService = siteService;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        public ActionResult Index(PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var forms = _formService.GetSubmissions(null, pager.GetStartIndex(), pager.PageSize).ToArray().GroupBy(x => x.FormName).ToArray();
            var pagerShape = Shape.Pager(pager).TotalItemCount(_formService.GetSubmissions().Count());
            var viewModel = new FormsIndexViewModel {
                Forms = forms,
                Pager = pagerShape
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