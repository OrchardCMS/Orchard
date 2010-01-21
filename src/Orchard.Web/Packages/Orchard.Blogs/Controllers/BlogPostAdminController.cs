using System;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Data;
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
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!_services.Authorizer.Authorize(Permissions.EditBlogPost, T("Not allowed to create blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
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

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            //TODO: (erikpo) Move this duplicate code somewhere else
            DateTime? publishDate = null;
            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            }
            else if (string.Equals(Request.Form["Command"], "PublishLater")) {
                DateTime publishDateValue;
                if (DateTime.TryParse(Request.Form["Published"], out publishDateValue)) {
                    publishDate = publishDateValue;
                }
            }

            model.BlogPost = _services.ContentManager.UpdateEditorModel(_services.ContentManager.New<BlogPost>(BlogPostDriver.ContentType.Name), this);
            model.BlogPost.Item.Blog = blog;
            if (!publishNow && publishDate != null)
                model.BlogPost.Item.Published = publishDate.Value;

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return View(model);
            }

            //todo: (heskew) make it so we no longer need to set as draft on create then publish (all to get the publishing & published events fired)
            _services.ContentManager.Create(model.BlogPost.Item.ContentItem, VersionOptions.Draft);

            if (publishNow)
                _services.ContentManager.Publish(model.BlogPost.Item.ContentItem);

            if (publishNow)
                _services.Notifier.Information(T("Blog post has been published"));
            else if (publishDate != null)
                _services.Notifier.Information(T("Blog post has been scheduled for publishing"));
            else
                _services.Notifier.Information(T("Blog post draft has been saved"));

            return Redirect(Url.BlogPostEdit(blogSlug, model.BlogPost.Item.Id));
        }

        public ActionResult Edit(string blogSlug, int postId) {
            if (!_services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
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

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(postId, VersionOptions.Latest);

            if (post == null)
                return new NotFoundResult();

            //TODO: (erikpo) Move this duplicate code somewhere else
            DateTime? publishDate = null;
            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            }
            else if (string.Equals(Request.Form["Command"], "PublishLater")) {
                DateTime publishDateValue;
                if (DateTime.TryParse(Request.Form["Published"], out publishDateValue)) {
                    publishDate = publishDateValue;
                }
            }

            //TODO: (erikpo) Move this duplicate code somewhere else
            if (publishNow)
                _blogPostService.Publish(post);
            else if (publishDate != null)
                _blogPostService.Publish(post, publishDate.Value);
            else {
                _blogPostService.Unpublish(post);
            }

            var model = new BlogPostEditViewModel {
                BlogPost = _services.ContentManager.UpdateEditorModel(post, this)
            };

            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();

                return View(model);
            }

            if (publishNow)
                _services.Notifier.Information(T("Blog post has been published"));
            else if (publishDate != null)
                _services.Notifier.Information(T("Blog post has been scheduled for publishing"));
            else
                _services.Notifier.Information(T("Blog post draft has been saved"));

            return Redirect(Url.BlogPostEdit(blogSlug, model.BlogPost.Item.Id));
        }

        [HttpPost]
        public ActionResult Delete(string blogSlug, int postId) {
            //refactoring: test PublishBlogPost/PublishOthersBlogPost in addition if published
            if (!_services.Authorizer.Authorize(Permissions.DeleteBlogPost, T("Couldn't delete blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
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

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
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