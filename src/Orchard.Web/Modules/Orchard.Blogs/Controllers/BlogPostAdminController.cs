using System;
using System.Reflection;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.AntiForgery;
using Orchard.Mvc.Extensions;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {

    /// <summary>
    /// TODO: (PH:Autoroute) This replicates a whole lot of Core.Contents functionality. All we actually need to do is take the BlogId from the query string in the BlogPostPartDriver, and remove
    /// helper extensions from UrlHelperExtensions.
    /// </summary>
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
        public Localizer T { get; set; }

        public ActionResult Create(int blogId) {

            var blog = _blogService.Get(blogId, VersionOptions.Latest).As<BlogPart>();
            if (blog == null)
                return HttpNotFound();

            var blogPost = Services.ContentManager.New<BlogPostPart>("BlogPost");
            blogPost.BlogPart = blog;

            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, blog, T("Not allowed to create blog post")))
                return new HttpUnauthorizedResult();

            var model = Services.ContentManager.BuildEditor(blogPost);
            
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Save")]
        public ActionResult CreatePOST(int blogId) {
            return CreatePOST(blogId, false);
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishPOST(int blogId) {
            if (!Services.Authorizer.Authorize(Permissions.PublishOwnBlogPost, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            return CreatePOST(blogId, true);
        }

        private ActionResult CreatePOST(int blogId, bool publish = false) {
            var blog = _blogService.Get(blogId, VersionOptions.Latest).As<BlogPart>();

            if (blog == null)
                return HttpNotFound();

            var blogPost = Services.ContentManager.New<BlogPostPart>("BlogPost");
            blogPost.BlogPart = blog;

            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, blog, T("Couldn't create blog post")))
                return new HttpUnauthorizedResult();
            
            Services.ContentManager.Create(blogPost, VersionOptions.Draft);
            var model = Services.ContentManager.UpdateEditor(blogPost, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            if (publish) {
                if (!Services.Authorizer.Authorize(Permissions.PublishBlogPost, blog.ContentItem, T("Couldn't publish blog post")))
                    return new HttpUnauthorizedResult();

                Services.ContentManager.Publish(blogPost.ContentItem);
            }

            Services.Notifier.Information(T("Your {0} has been created.", blogPost.TypeDefinition.DisplayName));
            return Redirect(Url.BlogPostEdit(blogPost));
        }

        //todo: the content shape template has extra bits that the core contents module does not (remove draft functionality)
        //todo: - move this extra functionality there or somewhere else that's appropriate?
        public ActionResult Edit(int blogId, int postId) {
            var blog = _blogService.Get(blogId, VersionOptions.Latest);
            if (blog == null)
                return HttpNotFound();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, post, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            var model = Services.ContentManager.BuildEditor(post);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int blogId, int postId, string returnUrl) {
            return EditPOST(blogId, postId, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    Services.ContentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public ActionResult EditAndPublishPOST(int blogId, int postId, string returnUrl) {
            var blog = _blogService.Get(blogId, VersionOptions.Latest);
            if (blog == null)
                return HttpNotFound();

            // Get draft (create a new version if needed)
            var blogPost = _blogPostService.Get(postId, VersionOptions.DraftRequired);
            if (blogPost == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishBlogPost, blogPost, T("Couldn't publish blog post")))
                return new HttpUnauthorizedResult();

            return EditPOST(blogId, postId, returnUrl, contentItem => Services.ContentManager.Publish(contentItem));
        }

        public ActionResult EditPOST(int blogId, int postId, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var blog = _blogService.Get(blogId, VersionOptions.Latest);
            if (blog == null)
                return HttpNotFound();

            // Get draft (create a new version if needed)
            var blogPost = _blogPostService.Get(postId, VersionOptions.DraftRequired);
            if (blogPost == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, blogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            // Validate form input
            var model = Services.ContentManager.UpdateEditor(blogPost, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            conditionallyPublish(blogPost.ContentItem);

            Services.Notifier.Information(T("Your {0} has been saved.", blogPost.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, Url.BlogPostEdit(blogPost));
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
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, draft, T("Couldn't discard blog post draft")))
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
            return RedirectToEdit(Services.ContentManager.GetLatest<BlogPostPart>(id));
        }

        ActionResult RedirectToEdit(IContent item) {
            if (item == null || item.As<BlogPostPart>() == null)
                return HttpNotFound();
            return RedirectToAction("Edit", new { BlogId = item.As<BlogPostPart>().BlogPart.Id, PostId = item.ContentItem.Id });
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Delete(int blogId, int postId) {
            //refactoring: test PublishBlogPost/PublishBlogPost in addition if published

            var blog = _blogService.Get(blogId, VersionOptions.Latest);
            if (blog == null)
                return HttpNotFound();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.DeleteBlogPost, post, T("Couldn't delete blog post")))
                return new HttpUnauthorizedResult();

            _blogPostService.Delete(post);
            Services.Notifier.Information(T("Blog post was successfully deleted"));

            return Redirect(Url.BlogForAdmin(blog.As<BlogPart>()));
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Publish(int blogId, int postId) {
            var blog = _blogService.Get(blogId, VersionOptions.Latest);
            if (blog == null)
                return HttpNotFound();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishBlogPost, post, T("Couldn't publish blog post")))
                return new HttpUnauthorizedResult();

            _blogPostService.Publish(post);
            Services.Notifier.Information(T("Blog post successfully published."));

            return Redirect(Url.BlogForAdmin(blog.As<BlogPart>()));
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Unpublish(int blogId, int postId) {
            var blog = _blogService.Get(blogId, VersionOptions.Latest);
            if (blog == null)
                return HttpNotFound();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishBlogPost, post, T("Couldn't unpublish blog post")))
                return new HttpUnauthorizedResult();

            _blogPostService.Unpublish(post);
            Services.Notifier.Information(T("Blog post successfully unpublished."));

            return Redirect(Url.BlogForAdmin(blog.As<BlogPart>()));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}