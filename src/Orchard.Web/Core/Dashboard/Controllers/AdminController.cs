using System.Web.Mvc;
using Orchard.DisplayManagement;

namespace Orchard.Core.Dashboard.Controllers {
    public class AdminController : Controller {
        private readonly IShapeHelperFactory _shapeHelperFactory;

        public AdminController(IShapeHelperFactory shapeHelperFactory) {
            _shapeHelperFactory = shapeHelperFactory;
        }

        public ActionResult Index() {
            var shape = _shapeHelperFactory.CreateHelper();
            var list = shape.List();
            var list2 = shape.List();

            list.Id = "the-list";
            list.Classes.Add("foo");
            list.Attributes["onclick"] = "etc";
            list.ItemClasses.Add("yarg");

            list.Add("one");
            list.Add("two");
            list.Add(list2);
            list.Add(shape.DumpShapeTable());
            list.Add("four");

            list2.Add("three a");
            list2.Add("three b");

            return View(list);
        }
    }
}