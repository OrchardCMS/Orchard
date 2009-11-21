using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Mvc.Results;

namespace Orchard.Blogs.Controllers {
    public class BlogPostController : Controller {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogPostController(IBlogService blogService, IBlogPostService blogPostService) {
            _blogService = blogService;
            _blogPostService = blogPostService;
        }

        public ActionResult ListByBlog(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            return View(_blogPostService.Get(blog));
        }

        //TODO: (erikpo) Should think about moving the slug parameters and get calls and null checks up into a model binder or action filter
        public ActionResult Item(string blogSlug, string postSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug);

            if (post == null)
                return new NotFoundResult();

            return View(post);
        }
    }
}