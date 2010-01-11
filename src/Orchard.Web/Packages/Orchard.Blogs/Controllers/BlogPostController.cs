using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogPostController : Controller, IUpdateModel {
        private readonly ISessionLocator _sessionLocator;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogPostController(
            IOrchardServices services,
            ISessionLocator sessionLocator,
            IBlogService blogService,
            IBlogPostService blogPostService) {
            Services = services;
            _sessionLocator = sessionLocator;
            _blogService = blogService;
            _blogPostService = blogPostService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        private Localizer T { get; set; }

        //TODO: (erikpo) Should think about moving the slug parameters and get calls and null checks up into a model binder or action filter
        public ActionResult Item(string blogSlug, string postSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ViewPost, T("Couldn't view blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            //TODO: (erikpo) Look up the current user and their permissions to this blog post and determine if they should be able to view it or not.
            VersionOptions versionOptions = VersionOptions.Published;
            BlogPost post = _blogPostService.Get(blog, postSlug, versionOptions);

            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostViewModel {
                Blog = blog,
                BlogPost = Services.ContentManager.BuildDisplayModel(post, "Detail")
            };

            return View(model);
        }

        public ActionResult ListByArchive(string blogSlug, string archiveData) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var archive = new ArchiveData(archiveData);
            var model = new BlogPostArchiveViewModel {
                Blog = blog,
                ArchiveData = archive,
                BlogPosts = _blogPostService.Get(blog, archive).Select(b => Services.ContentManager.BuildDisplayModel(b, "Summary"))
            };

            return View(model);
        }

        public ActionResult Slugify(string value) {
            string slug = value;

            //TODO: (erikpo) Move this into a utility class
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
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.CreatePost, T("Not allowed to create blog post")))
                return new HttpUnauthorizedResult();
            
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();
            
            var blogPost = Services.ContentManager.BuildEditorModel(Services.ContentManager.New<BlogPost>("blogpost"));
            blogPost.Item.Blog = blog;

            var model = new CreateBlogPostViewModel {
                BlogPost = blogPost
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(string blogSlug, CreateBlogPostViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.CreatePost, T("Couldn't create blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            }

            BlogPost blogPost = Services.ContentManager.Create<BlogPost>("blogpost", publishNow ? VersionOptions.Published : VersionOptions.Draft, bp => { bp.Blog = blog; });
            model.BlogPost = Services.ContentManager.UpdateEditorModel(blogPost, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                return View(model);
            }

            //TEMP: (erikpo) ensure information has committed for this record
            var session = _sessionLocator.For(typeof(BlogPostRecord));
            session.Flush();

            return Redirect(Url.BlogPost(blogSlug, model.BlogPost.Item.Slug));
        }

        public ActionResult Edit(string blogSlug, string postSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ModifyPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug, VersionOptions.Latest);

            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostEditViewModel {
                BlogPost = Services.ContentManager.BuildEditorModel(post)
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string blogSlug, string postSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ModifyPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug, VersionOptions.Latest);

            if (post == null)
                return new NotFoundResult();
            
            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            }

            if (publishNow)
                _blogPostService.Publish(post);
            else
                _blogPostService.Unpublish(post);

            var model = new BlogPostEditViewModel {
                BlogPost = Services.ContentManager.UpdateEditorModel(post, this)
            };

            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                return View(model);
            }

            Services.Notifier.Information(T("Blog post information updated."));

            return Redirect(Url.BlogPostEdit(blog.Slug, post.Slug));
        }

        [HttpPost]
        public ActionResult Delete(string blogSlug, string postSlug) {
            if (!Services.Authorizer.Authorize(Permissions.DeletePost, T("Couldn't delete blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug);

            if (post == null)
                return new NotFoundResult();

            _blogPostService.Delete(post);

            Services.Notifier.Information(T("Blog post was successfully deleted"));

            return Redirect(Url.BlogForAdmin(blogSlug));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}