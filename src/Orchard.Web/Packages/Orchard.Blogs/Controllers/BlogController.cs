using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Mvc.Results;

namespace Orchard.Blogs.Controllers {
    public class BlogController : Controller {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService) {
            _blogService = blogService;
        }

        public ActionResult List() {
            return View(_blogService.Get());
        }

        //TODO: (erikpo) Should think about moving the slug parameter and get call and null check up into a model binder or action filter
        public ActionResult Item(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            return View(blog);
        }
    }
}