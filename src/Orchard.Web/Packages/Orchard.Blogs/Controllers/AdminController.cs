using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;

namespace Orchard.Blogs.Controllers {
    public class AdminController : Controller {
        private readonly IBlogService _blogService;

        public AdminController(IBlogService blogService) {
            _blogService = blogService;
        }

        public ActionResult Create() {
            return View(new CreateBlogViewModel());
        }

        [HttpPost]
        public ActionResult Create(CreateBlogViewModel model) {
            if (!ModelState.IsValid)
                return View(model);

            Blog blog = _blogService.CreateBlog(model.ToCreateBlogParams());

            return RedirectToAction("edit", new { blog.Record.Id });
        }
    }
}
