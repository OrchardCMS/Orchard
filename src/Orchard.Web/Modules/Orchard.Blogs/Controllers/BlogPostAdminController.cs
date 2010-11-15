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
using Orchard.Mvc.AntiForgery;
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
        public Localizer T { get; set; }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, T("Not allowed to create blog post")))
                return new HttpUnauthorizedResult();

            var blogPost = Services.ContentManager.New<BlogPostPart>("BlogPost");
            if (blogPost.BlogPart == null)
                return HttpNotFound();

            var model = Services.ContentManager.BuildEditor(blogPost);

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Save")]
        public ActionResult CreatePOST() {
            return CreatePOST(contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    Services.ContentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishPOST() {
            return CreatePOST(contentItem => Services.ContentManager.Publish(contentItem));
        }

        public ActionResult CreatePOST(Action<ContentItem> conditionallyPublish) {
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't create blog post")))
                return new HttpUnauthorizedResult();

            var blogPost = Services.ContentManager.New<BlogPostPart>("BlogPost");
            if (blogPost.BlogPart == null)
                return HttpNotFound();

            Services.ContentManager.Create(blogPost, VersionOptions.Draft);
            var model = Services.ContentManager.UpdateEditor(blogPost, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            conditionallyPublish(blogPost.ContentItem);

            Services.Notifier.Information(T("Your {0} has been created.", blogPost.TypeDefinition.DisplayName));
            return Redirect(Url.BlogPostEdit(blogPost));
        }

        //todo: the content shape template has extra bits that the core contents module does not (remove draft functionality)
        //todo: - move this extra functionality there or somewhere else that's appropriate?
        public ActionResult Edit(string blogSlug, int postId) {
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return HttpNotFound();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return HttpNotFound();

            var model = Services.ContentManager.BuildEditor(post);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPOST(string blogSlug, int postId, string returnUrl) {
            return EditPOST(blogSlug, postId, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    Services.ContentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public ActionResult EditAndPublishPOST(string blogSlug, int postId, string returnUrl) {
            return EditPOST(blogSlug, postId, returnUrl, contentItem => Services.ContentManager.Publish(contentItem));
        }

        public ActionResult EditPOST(string blogSlug, int postId, string returnUrl, Action<ContentItem> conditionallyPublish) {
            if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return HttpNotFound();

            // Get draft (create a new version if needed)
            var blogPost = _blogPostService.Get(postId, VersionOptions.DraftRequired);
            if (blogPost == null)
                return HttpNotFound();

            // Validate form input
            var model = Services.ContentManager.UpdateEditor(blogPost, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            conditionallyPublish(blogPost.ContentItem);

            Services.Notifier.Information(T("Your {0} has been saved.", blogPost.TypeDefinition.DisplayName));

            if (!String.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return Redirect(Url.BlogPostEdit(blogPost));
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
            return RedirectToEdit(Services.ContentManager.GetLatest<BlogPostPart>(id));
        }

        ActionResult RedirectToEdit(IContent item) {
            if (item == null || item.As<BlogPostPart>() == null)
                return HttpNotFound();
            return RedirectToAction("Edit", new { BlogSlug = item.As<IRoutableAspect>().Path, PostId = item.ContentItem.Id });
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Delete(string blogSlug, int postId) {
            //refactoring: test PublishBlogPost/PublishOthersBlogPost in addition if published
            if (!Services.Authorizer.Authorize(Permissions.DeleteBlogPost, T("Couldn't delete blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return HttpNotFound();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return HttpNotFound();

            _blogPostService.Delete(post);
            Services.Notifier.Information(T("Blog post was successfully deleted"));

            return Redirect(Url.BlogForAdmin(blog));
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Publish(string blogSlug, int postId) {
            if (!Services.Authorizer.Authorize(Permissions.PublishBlogPost, T("Couldn't publish blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return HttpNotFound();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return HttpNotFound();

            _blogPostService.Publish(post);
            Services.Notifier.Information(T("Blog post successfully published."));

            return Redirect(Url.BlogForAdmin(blog));
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Unpublish(string blogSlug, int postId) {
            if (!Services.Authorizer.Authorize(Permissions.PublishBlogPost, T("Couldn't unpublish blog post")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return HttpNotFound();

            var post = _blogPostService.Get(postId, VersionOptions.Latest);
            if (post == null)
                return HttpNotFound();

            _blogPostService.Unpublish(post);
            Services.Notifier.Information(T("Blog post successfully unpublished."));

            return Redirect(Url.BlogForAdmin(blog));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

    public class FormValueRequiredAttribute : ActionMethodSelectorAttribute {
        private readonly string _submitButtonName;

        public FormValueRequiredAttribute(string submitButtonName) {
            _submitButtonName = submitButtonName;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
            var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
            return !string.IsNullOrEmpty(value);
        }
    }
}