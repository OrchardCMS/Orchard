using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogPostAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _services;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogPostAdminController(IOrchardServices services, IBlogService blogService, IBlogPostService blogPostService) {
            _services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public ActionResult Create(string blogSlug) {
            if (!_services.Authorizer.Authorize(Permissions.EditBlogPost, T("Not allowed to create blog post")))
                return new HttpUnauthorizedResult();

            Blog blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var blogPost = _services.ContentManager.New<BlogPost>(BlogPostDriver.ContentType.Name);
            blogPost.Blog = blog;

            var model = new CreateBlogPostViewModel {
                BlogPost = _services.ContentManager.BuildEditorModel(blogPost)
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(string blogSlug, CreateBlogPostViewModel model) {
            if (!_services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't create blog post")))
                return new HttpUnauthorizedResult();

            Blog blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            // Validate form input
            var blogPost = _services.ContentManager.New<BlogPost>(BlogPostDriver.ContentType.Name);
            blogPost.Blog = blog;
            model.BlogPost = _services.ContentManager.UpdateEditorModel(blogPost, this);

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return View(model);
            }

            _services.ContentManager.Create(model.BlogPost.Item.ContentItem, VersionOptions.Draft);

            // Execute publish command
            switch (Request.Form["Command"]) {
                case "PublishNow":
                    _blogPostService.Publish(model.BlogPost.Item);
                    _services.Notifier.Information(T("Blog post has been published"));
                    break;
                case "PublishLater":
                    _blogPostService.Publish(model.BlogPost.Item, model.BlogPost.Item.ScheduledPublishUtc.Value);
                    _services.Notifier.Information(T("Blog post has been scheduled for publishing"));
                    break;
                default:
                    _services.Notifier.Information(T("Blog post draft has been saved"));
                    break;
            }

            return Redirect(Url.BlogPostEdit(blogSlug, model.BlogPost.Item.Id));
        }

        public ActionResult Edit(string blogSlug, int postId) {
            if (!_services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            Blog blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostEditViewModel {
                BlogPost = _services.ContentManager.BuildEditorModel(post)
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string blogSlug, int postId) {
            if (!_services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            Blog blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            // Get draft (create a new version if needed)
            BlogPost post = _blogPostService.Get(postId, VersionOptions.DraftRequired);
            if (post == null)
                return new NotFoundResult();

            // Validate form input
            var model = new BlogPostEditViewModel {
                BlogPost = _services.ContentManager.UpdateEditorModel(post, this)
            };

            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return View(model);
            }

            // Execute publish command
            switch (Request.Form["Command"]) {
                case "PublishNow":
                    _blogPostService.Publish(model.BlogPost.Item);
                    _services.Notifier.Information(T("Blog post has been published"));
                    break;
                case "PublishLater":
                    _blogPostService.Publish(model.BlogPost.Item, model.BlogPost.Item.ScheduledPublishUtc.Value);
                    _services.Notifier.Information(T("Blog post has been scheduled for publishing"));
                    break;
                default:
                    //_blogPostService.Unpublish(model.BlogPost.Item);
                    _services.Notifier.Information(T("Blog post draft has been saved"));
                    break;
            }

            return Redirect(Url.BlogPostEdit(blogSlug, model.BlogPost.Item.Id));
        }

        [HttpPost]
        public ActionResult Delete(string blogSlug, int postId) {
            //refactoring: test PublishBlogPost/PublishOthersBlogPost in addition if published
            if (!_services.Authorizer.Authorize(Permissions.DeleteBlogPost, T("Couldn't delete blog post")))
                return new HttpUnauthorizedResult();

            Blog blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return new NotFoundResult();

            _blogPostService.Delete(post);
            _services.Notifier.Information(T("Blog post was successfully deleted"));

            return Redirect(Url.BlogForAdmin(blogSlug));
        }

        [HttpPost]
        public ActionResult Publish(string blogSlug, int postId) {
            if (!_services.Authorizer.Authorize(Permissions.PublishBlogPost, T("Couldn't publish blog post")))
                return new HttpUnauthorizedResult();

            Blog blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return new NotFoundResult();

            _blogPostService.Publish(post);
            _services.Notifier.Information(T("Blog post information updated."));

            return Redirect(Url.BlogForAdmin(blog.Slug));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}