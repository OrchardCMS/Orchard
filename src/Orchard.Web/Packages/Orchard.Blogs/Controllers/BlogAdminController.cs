using System.Linq;
using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Models;
using Orchard.Mvc.Results;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogAdminController : Controller {
        private readonly IBlogService _blogService;
        private readonly IContentManager _contentManager;

        public BlogAdminController(IContentManager contentManager, IBlogService blogService) {
            _contentManager = contentManager;
            _blogService = blogService;
        }

        public ActionResult List() {
            //TODO: (erikpo) Need to make templatePath be more convention based so if my controller name has "Admin" in it then "Admin/{type}" is assumed
            var model = new AdminBlogsViewModel {
                Blogs = _blogService.Get().Select(b => _contentManager.BuildDisplayModel(b, "SummaryAdmin"))
            };

            return View(model);
        }

        //TODO: (erikpo) Should move the slug parameter and get call and null check up into a model binder
        public ActionResult Item(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            //TODO: (erikpo) Need to make templatePath be more convention based so if my controller name has "Admin" in it then "Admin/{type}" is assumed
            var model = new BlogForAdminViewModel {
                Blog = _contentManager.BuildDisplayModel(blog, "DetailAdmin")
            };

            return View(model);
        }
    }
}