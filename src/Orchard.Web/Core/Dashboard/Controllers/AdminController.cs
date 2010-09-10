using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.Security;

namespace Orchard.Core.Dashboard.Controllers {
    public class AdminController : Controller {
        private readonly IShapeHelperFactory _shapeHelperFactory;
        private readonly IContentManager _contentManager;

        public AdminController(IShapeHelperFactory shapeHelperFactory, IContentManager contentManager) {
            _shapeHelperFactory = shapeHelperFactory;
            _contentManager = contentManager;
        }

        public virtual IUser CurrentUser { get; set; }

        public ActionResult Index() {
            var shape = _shapeHelperFactory.CreateHelper();

            var list = shape.List();

            list.Add(_contentManager.BuildDisplayModel(CurrentUser, ""));
            foreach (var contentItem in _contentManager.Query().Join<BodyPartRecord>().List()) {
                list.Add(_contentManager.BuildDisplayModel(contentItem, "Summary"));
            }

            //
            //var list2 = shape.List();

            //list.Id = "the-list";
            //list.Classes.Add("foo");
            //list.Attributes["onclick"] = "etc";
            //list.ItemClasses.Add("yarg");

            //list.Add("one");
            //list.Add("two");
            //list.Add(list2);
            //list.Add(shape.DumpShapeTable());
            //list.Add("four");

            //list2.Add("three a");
            //list2.Add("three b");)))

            return View(list);
        }
    }
}