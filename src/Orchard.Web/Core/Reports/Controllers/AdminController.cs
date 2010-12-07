using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Reports.ViewModels;
using Orchard.Reports.Services;

namespace Orchard.Core.Reports.Controllers {
    public class AdminController : Controller {
        private readonly IReportsManager _reportsManager;

        public AdminController(IReportsManager reportsManager) {
            _reportsManager = reportsManager;
        }


        public ActionResult Index() {
            var model = new ReportsAdminIndexViewModel { Reports = _reportsManager.GetReports().ToList() };

            return View(model);
        }

        public ActionResult Display(int id) {
            var model = new DisplayReportViewModel { Report = _reportsManager.Get(id) };

            return View(model);
        }

    }
}