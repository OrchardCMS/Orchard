using System;
using System.Web.Mvc;
using Orchard.Data.Migration.Generator;
using Orchard.Localization;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.DevTools.Controllers {
    [ValidateInput(false)]
    public class DataMigrationController : Controller {
        private readonly ISchemaCommandGenerator _schemaCommandGenerator;

        public DataMigrationController(ISchemaCommandGenerator schemaCommandGenerator, IOrchardServices orchardServices) {
            _schemaCommandGenerator = schemaCommandGenerator;
            Services = orchardServices;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            return View(new BaseViewModel());
        }

        public ActionResult UpdateDatabase() {
            try {

                _schemaCommandGenerator.UpdateDatabase();
                Services.Notifier.Information(T("Database updated successfuly"));
            }
            catch (Exception ex) {
                Services.Notifier.Error(T("An error occured while updating the database: {0}", ex.Message));
            }

            return RedirectToAction("Index");            
        }
    }
}