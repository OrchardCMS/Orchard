using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogPostController : Controller, IUpdateModel {
        private readonly ISessionLocator _sessionLocator;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogPostController(ISessionLocator sessionLocator, IContentManager contentManager, INotifier notifier, IBlogService blogService, IBlogPostService blogPostService) {
            _sessionLocator = sessionLocator;
            _contentManager = contentManager;
            _notifier = notifier;
            _blogService = blogService;
            _blogPostService = blogPostService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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

            return View(new BlogPostViewModel {Blog = blog, Post = post, Displays = _contentManager.GetDisplays(post.ContentItem)});
        }

        public ActionResult Create(string blogSlug) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            return View(new CreateBlogPostViewModel {Blog = blog});
        }

        [HttpPost]
        public ActionResult Create(string blogSlug, CreateBlogPostViewModel model) {
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

        public ActionResult Edit(string blogSlug, string postSlug) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug);

            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostEditViewModel { Blog = blog, Post = post };
            model.Editors = _contentManager.GetEditors(model.Post.ContentItem);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string blogSlug, string postSlug, FormCollection input) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug);

            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostEditViewModel { Blog = blog, Post = post };
            model.Editors = _contentManager.UpdateEditors(model.Post.ContentItem, this);

            IValueProvider values = input.ToValueProvider();
            if (!TryUpdateModel(model, values))
                return View(model);

            _notifier.Information(T("Blog post information updated"));

            //TODO: (erikpo) Since the model isn't actually updated yet and it's possible the slug changed I'm getting the slug from input. Lame?!?!
            return Redirect(Url.BlogPostEdit(blog.Slug, values.GetValue(ControllerContext, "Slug").AttemptedValue));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
    }
}