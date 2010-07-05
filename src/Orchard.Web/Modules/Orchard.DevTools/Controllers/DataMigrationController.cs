using System.Web.Mvc;
using Orchard.Data.Migration.Generator;
using Orchard.DevTools.ViewModels;

namespace Orchard.DevTools.Controllers {
    [ValidateInput(false)]
    public class DataMigrationController : Controller {
        private readonly ISchemaCommandGenerator _schemaCommandGenerator;

        public DataMigrationController(ISchemaCommandGenerator schemaCommandGenerator) {
            _schemaCommandGenerator = schemaCommandGenerator;
        }

        public ActionResult Index() {
            var model = new DataMigrationIndexViewModel ();

            _schemaCommandGenerator.UpdateDatabase();

            return View(model);
        }

    }
}