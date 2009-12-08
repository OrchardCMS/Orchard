using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;
using Orchard.Mvc.Results;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogController : Controller, IUpdateModel {
        private readonly ISessionLocator _sessionLocator;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;
        private readonly IBlogService _blogService;

        public BlogController(ISessionLocator sessionLocator, IContentManager contentManager,
                              IAuthorizer authorizer, INotifier notifier,
                              IBlogService blogService) {
            _sessionLocator = sessionLocator;
            _contentManager = contentManager;
            _authorizer = authorizer;
            _notifier = notifier;
            _blogService = blogService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult List() {
            var model = new BlogsViewModel {
                Blogs = _blogService.Get().Select(b => _contentManager.GetDisplayViewModel(b, null, "Summary"))
            };

            return View(model);
        }

        public ActionResult ListForAdmin() {
            var model = new BlogsForAdminViewModel {
                Blogs = _blogService.Get().Select(b => _contentManager.GetDisplayViewModel(b, null, "SummaryAdmin"))
            };

            return View(model);
        }

        //TODO: (erikpo) Should move the slug parameter and get call and null check up into a model binder
        public ActionResult Item(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var model = new BlogViewModel {
                Blog = _contentManager.GetDisplayViewModel(blog, null, "Detail")
            };

            return View(model);
        }

        //TODO: (erikpo) Should move the slug parameter and get call and null check up into a model binder
        public ActionResult ItemForAdmin(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var model = new BlogForAdminViewModel {
                Blog = _contentManager.GetDisplayViewModel(blog, null, "DetailAdmin")
            };

            return View(model);
        }

        public ActionResult Create() {
            return View(new CreateBlogViewModel());
        }

        [HttpPost]
        public ActionResult Create(CreateBlogViewModel model) {
            if (!_authorizer.Authorize(Permissions.CreateBlog, T("Couldn't create blog")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid)
                return View(model);

            Blog blog = _blogService.Create(model.ToCreateBlogParams());

            //TEMP: (erikpo) ensure information has committed for this record
            var session = _sessionLocator.For(typeof(BlogRecord));
            session.Flush();

            return Redirect(Url.BlogForAdmin(blog.Slug));
        }

        public ActionResult Edit(string blogSlug) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var model = new BlogEditViewModel { Blog = blog };
            model.ItemView = _contentManager.GetEditorViewModel(model.Blog.ContentItem, "");
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string blogSlug, FormCollection input) {
            if (!_authorizer.Authorize(Permissions.ModifyBlog, T("Couldn't edit blog")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var model = new BlogEditViewModel { Blog = blog };
            model.ItemView = _contentManager.UpdateEditorViewModel(model.Blog.ContentItem, "", this);

            IValueProvider values = input.ToValueProvider();
            if (!TryUpdateModel(model, values))
                return View(model);

            _notifier.Information(T("Blog information updated"));

            return Redirect(Url.BlogsForAdmin());
        }

        //[HttpPost] <- todo: (heskew) make all add/edit/remove POST only and verify the AntiForgeryToken
        public ActionResult Delete(string blogSlug) {
            if (!_authorizer.Authorize(Permissions.DeleteBlog, T("Couldn't delete blog")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            _blogService.Delete(blog);

            _notifier.Information(T("Blog was successfully deleted"));

            return Redirect(Url.BlogsForAdmin());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

    }
}