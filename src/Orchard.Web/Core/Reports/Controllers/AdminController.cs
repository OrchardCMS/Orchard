using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Reports.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Reports.Services;

namespace Orchard.Core.Reports.Controllers {
    public class AdminController : Controller {
        private readonly IReportsManager _reportsManager;

        public AdminController(IReportsManager reportsManager, IShapeHelperFactory shapeHelperFactory) {
            _reportsManager = reportsManager;
        }

        dynamic Shape { get; set; }

        public ActionResult Index() {
            var model = new ReportsAdminIndexViewModel { Reports = _reportsManager.GetReports().ToList() };

            return View(Shape.Model(model));
        }

        public ActionResult Display(int id) {
            var model = new DisplayReportViewModel { Report = _reportsManager.Get(id) };

            return View(Shape.Model(model));
        }

    }
}