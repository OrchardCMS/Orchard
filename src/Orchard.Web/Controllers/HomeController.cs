using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Mvc.ModelBinders;

namespace Orchard.Web.Controllers {
    [HandleError]
    public class HomeController : Controller {

        static HomeController() {
            
            var fooListBinder = new KeyedListModelBinder<Foo>(
                ModelBinders.Binders, ModelMetadataProviders.Current, x => x.Name);

            ModelBinders.Binders.Add(typeof(IList<Foo>), fooListBinder);
        }

        public ActionResult Index() {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About() {

            var foos = new[] {
                new Foo {Name = "one", Content = "uno"},
                new Foo {Name = "two", Content = "dos"},
                new Foo {Name = "three", Content = "tres"},
            };
            return View(new HomeAboutViewModel { Foos = foos });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult About(FormCollection input) {
            var foos = new[] {
                new Foo {Name = "one", Content = "uno"},
                new Foo {Name = "two", Content = "dos"},
                new Foo {Name = "three", Content = "tres"},
            };
            var vm = new HomeAboutViewModel { Foos = foos };
            UpdateModel(vm, input.ToValueProvider());
            return RedirectToAction("About");
        }
    }

    public class Foo {
        public string Name { get; set; }
        public string Content { get; set; }
    }

    public class HomeAboutViewModel {
        public IList<Foo> Foos { get; set; }
    }
}