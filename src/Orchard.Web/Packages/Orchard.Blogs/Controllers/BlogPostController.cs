using System.Text.RegularExpressions;
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
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogPostController : Controller, IUpdateModel {
        private readonly ISessionLocator _sessionLocator;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogPostController(ISessionLocator sessionLocator, IContentManager contentManager, 
                                  IAuthorizer authorizer, INotifier notifier, 
                                  IBlogService blogService, IBlogPostService blogPostService) {
            _sessionLocator = sessionLocator;
            _contentManager = contentManager;
            _authorizer = authorizer;
            _notifier = notifier;
            _blogService = blogService;
            _blogPostService = blogPostService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        //TODO: (erikpo) Should think about moving the slug parameters and get calls and null checks up into a model binder or action filter
        public ActionResult Item(string blogSlug, string postSlug) {
            if (!_authorizer.Authorize(Permissions.ViewPost, T("Couldn't view blog post")))
                return new HttpUnauthorizedResult();

            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug);

            if (post == null)
                return new NotFoundResult();

            return View(new BlogPostViewModel { Blog = blog, Post = post, ItemView = _contentManager.GetDisplayViewModel(post.ContentItem, null, "detail") });
        }

        public ActionResult Slugify(string value) {
            string slug = value;

            if (!string.IsNullOrEmpty(value)) {
                Regex regex = new Regex("([^a-z0-9-_]?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                slug = value.Trim();
                slug = slug.Replace(' ', '-');
                slug = slug.Replace("---", "-");
                slug = slug.Replace("--", "-");
                slug = regex.Replace(slug, "");

                if (slug.Length * 2 < value.Length)
                    return Json("");

                if (slug.Length > 100)
                    slug = slug.Substring(0, 100);
            }

            return Json(slug);
        }

        public ActionResult Create(string blogSlug) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var model = new CreateBlogPostViewModel {
                Blog = blog,
                ItemView = _contentManager.GetEditorViewModel(_contentManager.New("blogpost"), null)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(string blogSlug, CreateBlogPostViewModel model) {
            if (!_authorizer.Authorize(Permissions.CreatePost, T("Couldn't create blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            if (ModelState.IsValid == false) {
                model.Blog = blog;
                model.ItemView = _contentManager.UpdateEditorViewModel(_contentManager.New<BlogPost>("blogpost"), null, this);
                return View(model);
            }

            BlogPost blogPost = _blogPostService.Create(model.ToCreateBlogPostParams(blog));
            model.ItemView = _contentManager.UpdateEditorViewModel(blogPost, null, this);

            //TEMP: (erikpo) ensure information has committed for this record
            var session = _sessionLocator.For(typeof(BlogPostRecord));
            session.Flush();

            return Redirect(Url.BlogPost(blogSlug, blogPost.As<RoutableAspect>().Slug));
        }

        public ActionResult Edit(string blogSlug, string postSlug) {
            if (!_authorizer.Authorize(Permissions.ModifyPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug);

            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostEditViewModel { Blog = blog, Post = post };
            model.ItemView = _contentManager.GetEditorViewModel(model.Post.ContentItem, null);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string blogSlug, string postSlug, FormCollection input) {
            if (!_authorizer.Authorize(Permissions.ModifyPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug);

            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostEditViewModel { Blog = blog, Post = post };
            model.ItemView = _contentManager.UpdateEditorViewModel(model.Post, null, this);

            IValueProvider values = input.ToValueProvider();
            TryUpdateModel(model, values);

            _notifier.Information(T("Blog post information updated."));

            //TODO: (erikpo) Since the model isn't actually updated yet and it's possible the slug changed I'm getting the slug from input. Lame?!?!
            return Redirect(Url.BlogPostEdit(blog.Slug, values.GetValue(ControllerContext, "Slug").AttemptedValue));
        }

        [HttpPost]
        public ActionResult Delete(string blogSlug, string postSlug) {
            if (!_authorizer.Authorize(Permissions.DeletePost, T("Couldn't delete blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug);

            if (post == null)
                return new NotFoundResult();

            _blogPostService.Delete(post);

            _notifier.Information(T("Blog post was successfully deleted"));

            return Redirect(Url.BlogForAdmin(blogSlug));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
    }
}