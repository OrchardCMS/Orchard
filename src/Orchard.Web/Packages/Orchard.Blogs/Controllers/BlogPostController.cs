using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models;
using Orchard.Mvc.Results;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogPostController : Controller {
        private readonly ISessionLocator _sessionLocator;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogPostController(ISessionLocator sessionLocator, IBlogService blogService, IBlogPostService blogPostService) {
            _sessionLocator = sessionLocator;
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

            return View(new BlogPostViewModel {Blog = blog, Post = post});
        }

        public ActionResult Create(string blogSlug)
        {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            return View(new CreateBlogPostViewModel() {Blog = blog});
        }

        [HttpPost]
        public ActionResult Create(string blogSlug, CreateBlogPostViewModel model)
        {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            if (!ModelState.IsValid)
                return View(model);

            BlogPost blogPost = _blogPostService.Create(model.ToCreateBlogPostParams(blog));

            //TEMP: (erikpo) ensure information has committed for this record
            var session = _sessionLocator.For(typeof(BlogPostRecord));
            session.Flush();

            return Redirect(Url.BlogPost(blogSlug, blogPost.As<RoutableAspect>().Slug));
        }
    }
}