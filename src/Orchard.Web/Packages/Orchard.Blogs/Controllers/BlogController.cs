using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Mvc.Results;

namespace Orchard.Blogs.Controllers {
    public class BlogController : Controller {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogController(IBlogService blogService, IBlogPostService blogPostService) {
            _blogService = blogService;
            _blogPostService = blogPostService;
        }

        public ActionResult List() {
            return View(new BlogsViewModel {Blogs = _blogService.Get()});
        }

        //TODO: (erikpo) Should think about moving the slug parameter and get call and null check up into a model binder or action filter
        public ActionResult Item(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            IEnumerable<BlogPost> posts = _blogPostService.Get(blog);

            return View(new BlogViewModel {Blog = blog, Posts = posts});
        }
    }
}