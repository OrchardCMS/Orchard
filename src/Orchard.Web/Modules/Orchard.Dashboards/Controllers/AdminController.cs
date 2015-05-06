using System.Web.Mvc;
using Orchard.Dashboards.Services;
using Orchard.Mvc;

namespace Orchard.Dashboards.Controllers {
    public class AdminController : Controller {
        private readonly IDashboardService _dashboardService;
        public AdminController(IDashboardService dashboardService) {
            _dashboardService = dashboardService;
        }

        public ActionResult Index() {
            var shape = _dashboardService.GetDashboardShape();
            return new ShapeResult(this, shape);
        }
    }
}