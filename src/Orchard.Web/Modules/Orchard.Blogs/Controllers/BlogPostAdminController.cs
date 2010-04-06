using System.Web.Mvc;
using Orchard.Blogs.Drivers;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.AntiForgery;
using Orchard.Mvc.Results;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false), Admin]
    public class BlogPostAdminController : Controller, IUpdateModel {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogPostAdminController(IOrchardServices services, IBlogService blogService, IBlogPostService blogPostService) {
            Services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        private Localizer T { get; set; }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, T("Not allowed to create blog post")))
                return new HttpUnauthorizedResult();

            var blogPost = Services.ContentManager.New<BlogPost>(BlogPostDriver.ContentType.Name);

            if (blogPost.Blog == null)
                return new NotFoundResult();

            var model = new CreateBlogPostViewModel {
                BlogPost = Services.ContentManager.BuildEditorModel(blogPost)
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateBlogPostViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't create blog post")))
                return new HttpUnauthorizedResult();

            var blogPost = Services.ContentManager.New<BlogPost>(BlogPostDriver.ContentType.Name);
            
            if (blogPost.Blog == null)
                return new NotFoundResult();

            model.BlogPost = Services.ContentManager.UpdateEditorModel(blogPost, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.ContentManager.Create(model.BlogPost.Item.ContentItem, VersionOptions.Draft);
            Services.ContentManager.UpdateEditorModel(blogPost, this);

            // Execute publish command
            switch (Request.Form["Command"]) {
                case "PublishNow":
                    _blogPostService.Publish(model.BlogPost.Item);
                    Services.Notifier.Information(T("Blog post has been published"));
                    break;
                case "PublishLater":
                    _blogPostService.Publish(model.BlogPost.Item, model.BlogPost.Item.ScheduledPublishUtc.Value);
                    Services.Notifier.Information(T("Blog post has been scheduled for publishing"));
                    break;
                default:
                    Services.Notifier.Information(T("Blog post draft has been saved"));
                    break;
            }

            return Redirect(Url.BlogPostEdit(model.BlogPost.Item));
        }

        public ActionResult Edit(string blogSlug, int postId) {
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostEditViewModel {
                BlogPost = Services.ContentManager.BuildEditorModel(post)
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string blogSlug, int postId) {
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            // Get draft (create a new version if needed)
            var post = _blogPostService.Get(postId, VersionOptions.DraftRequired);
            if (post == null)
                return new NotFoundResult();

            // Validate form input
            var model = new BlogPostEditViewModel {
                BlogPost = Services.ContentManager.UpdateEditorModel(post, this)
            };

            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            // Execute publish command
            switch (Request.Form["Command"]) {
                case "PublishNow":
                    _blogPostService.Publish(model.BlogPost.Item);
                    Services.Notifier.Information(T("Blog post has been published"));
                    break;
                case "PublishLater":
                    _blogPostService.Publish(model.BlogPost.Item, model.BlogPost.Item.ScheduledPublishUtc.Value);
                    Services.Notifier.Information(T("Blog post has been scheduled for publishing"));
                    break;
                default:
                    //_blogPostService.Unpublish(model.BlogPost.Item);
                    Services.Notifier.Information(T("Blog post draft has been saved"));
                    break;
            }

            return Redirect(Url.BlogPostEdit(model.BlogPost.Item));
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult DiscardDraft(int id) {
            // get the current draft version
            var draft = Services.ContentManager.Get(id, VersionOptions.Draft);
            if (draft == null) {
                Services.Notifier.Information(T("There is no draft to discard."));
                return RedirectToEdit(id);
            }

            // check edit permission
            if (!Services.Authorizer.Authorize(Permissions.EditOthersBlogPost, draft, T("Couldn't discard blog post draft")))
                return new HttpUnauthorizedResult();

            // locate the published revision to revert onto
            var published = Services.ContentManager.Get(id, VersionOptions.Published);
            if (published == null) {
                Services.Notifier.Information(T("Can not discard draft on unpublished blog post."));
                return RedirectToEdit(draft);
            }

            // marking the previously published version as the latest
            // has the effect of discarding the draft but keeping the history
            draft.VersionRecord.Latest = false;
            published.VersionRecord.Latest = true;

            Services.Notifier.Information(T("Blog post draft version discarded"));
            return RedirectToEdit(published);
        }

        ActionResult RedirectToEdit(int id) {
            return RedirectToEdit(Services.ContentManager.GetLatest<BlogPost>(id));
        }

        ActionResult RedirectToEdit(IContent item) {
            if (item == null || item.As<BlogPost>() == null)
                return new NotFoundResult();
            return RedirectToAction("Edit", new { BlogSlug = item.As<BlogPost>().Blog.Slug, PostId = item.ContentItem.Id });
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Delete(string blogSlug, int postId) {
            //refactoring: test PublishBlogPost/PublishOthersBlogPost in addition if published
            if (!Services.Authorizer.Authorize(Permissions.DeleteBlogPost, T("Couldn't delete blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return new NotFoundResult();

            _blogPostService.Delete(post);
            Services.Notifier.Information(T("Blog post was successfully deleted"));

            return Redirect(Url.BlogForAdmin(blogSlug));
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Publish(string blogSlug, int postId) {
            if (!Services.Authorizer.Authorize(Permissions.PublishBlogPost, T("Couldn't publish blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return new NotFoundResult();

            _blogPostService.Publish(post);
            Services.Notifier.Information(T("Blog post successfully published."));

            return Redirect(Url.BlogForAdmin(blog.Slug));
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Unpublish(string blogSlug, int postId) {
            if (!Services.Authorizer.Authorize(Permissions.PublishBlogPost, T("Couldn't unpublish blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return new NotFoundResult();

            _blogPostService.Unpublish(post);
            Services.Notifier.Information(T("Blog post successfully unpublished."));

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