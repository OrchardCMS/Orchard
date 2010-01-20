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
        private readonly ISessionLocator _sessionLocator;

        public BlogPostAdminController(IOrchardServices services, IBlogService blogService, IBlogPostService blogPostService, ISessionLocator sessionLocator) {
            _services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _sessionLocator = sessionLocator;
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

            var blogPost = _services.ContentManager.New<BlogPost>("blogpost");
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
            } else if (string.Equals(Request.Form["Publish"], "Publish")) {
                DateTime publishDateValue;
                if (DateTime.TryParse(Request.Form["Publish"], out publishDateValue)) {
                    publishDate = publishDateValue;
                }
            }

            model.BlogPost = _services.ContentManager.UpdateEditorModel(_services.ContentManager.New<BlogPost>("blogpost"), this);
            model.BlogPost.Item.Blog = blog;
            if (!publishNow && publishDate != null)
                model.BlogPost.Item.Published = publishDate.Value;

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();

                return View(model);
            }

            //TODO: (erikpo) Evaluate if publish options should be moved into create or out of create to keep it clean
            _services.ContentManager.Create(model.BlogPost.Item.ContentItem, publishNow ? VersionOptions.Published : VersionOptions.Draft);

            //TEMP: (erikpo) ensure information has committed for this record
            var session = _sessionLocator.For(typeof(ContentItemRecord));
            session.Flush();

            return Redirect(Url.BlogPost(blogSlug, model.BlogPost.Item.As<RoutableAspect>().Slug));
        }

        public ActionResult Edit(string blogSlug, string postSlug) {
            if (!_services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug, VersionOptions.Latest);

            if (post == null)
                return new NotFoundResult();

            var model = new BlogPostEditViewModel {
                BlogPost = _services.ContentManager.BuildEditorModel(post)
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string blogSlug, string postSlug) {
            if (!_services.Authorizer.Authorize(Permissions.EditBlogPost, T("Couldn't edit blog post")))
                return new HttpUnauthorizedResult();

            bool isDraft = false;

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug, VersionOptions.Latest);

            if (post == null)
                return new NotFoundResult();

            //TODO: (erikpo) Move this duplicate code somewhere else
            DateTime? publishDate = null;
            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            } else if (string.Equals(Request.Form["Publish"], "Publish")) {
                DateTime publishDateValue;
                if (DateTime.TryParse(Request.Form["Publish"], out publishDateValue)) {
                    publishDate = publishDateValue;
                }
            }

            //TODO: (erikpo) Move this duplicate code somewhere else
            if (publishNow)
                _blogPostService.Publish(post);
            else if (publishDate != null)
                _blogPostService.Publish(post, publishDate.Value);
            else {
                isDraft = true;
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

            _services.Notifier.Information(T("Blog post information updated."));

            if (isDraft) {
                return Redirect(Url.BlogPostEdit(blog.Slug, post.Slug));
            }
            return Redirect(Url.BlogForAdmin(blog.Slug));
        }

        [HttpPost]
        public ActionResult Delete(string blogSlug, string postSlug) {
            //refactoring: test PublishBlogPost/PublishOthersBlogPost in addition if published
            if (!_services.Authorizer.Authorize(Permissions.DeleteBlogPost, T("Couldn't delete blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug, VersionOptions.Latest);

            if (post == null)
                return new NotFoundResult();

            _blogPostService.Delete(post);

            _services.Notifier.Information(T("Blog post was successfully deleted"));

            return Redirect(Url.BlogForAdmin(blogSlug));
        }

        public ActionResult Publish(string blogSlug, string postSlug) {
            if (!_services.Authorizer.Authorize(Permissions.PublishPost, T("Couldn't publish blog post")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            BlogPost post = _blogPostService.Get(blog, postSlug, VersionOptions.Latest);

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