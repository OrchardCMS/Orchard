using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Blogs.Drivers;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false), Admin]
    public class BlogAdminController : Controller, IUpdateModel {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IBlogSlugConstraint _blogSlugConstraint;

        public BlogAdminController(IOrchardServices services,
            IBlogService blogService,
            IBlogPostService blogPostService,
            IContentManager contentManager,
            ITransactionManager transactionManager,
            IBlogSlugConstraint blogSlugConstraint) {
            Services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _blogSlugConstraint = blogSlugConstraint;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Create() {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Not allowed to create blogs")))
                return new HttpUnauthorizedResult();

            var blog = Services.ContentManager.New<BlogPart>(BlogPartDriver.ContentType.Name);
            if (blog == null)
                return new NotFoundResult();

            var model = new CreateBlogViewModel {
                Blog = Services.ContentManager.BuildEditorModel(blog)
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateBlogViewModel model) {
            var blog = Services.ContentManager.New<BlogPart>(BlogPartDriver.ContentType.Name);

            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't create blog")))
                return new HttpUnauthorizedResult();

            _blogService.Create(blog);
            model.Blog = _contentManager.UpdateEditorModel(blog, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _blogSlugConstraint.AddSlug(model.Blog.Item.Slug);

            return Redirect(Url.BlogForAdmin(model.Blog.Item.Slug));
        }

        public ActionResult Edit(string blogSlug) {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Not allowed to edit blog")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var model = new BlogEditViewModel {
                Blog = Services.ContentManager.BuildEditorModel(blog),
                //PromoteToHomePage = CurrentSite.HomePage == "BlogHomePageProvider;" + blog.Id
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string blogSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't edit blog")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var model = new BlogEditViewModel {
                Blog = Services.ContentManager.UpdateEditorModel(blog, this),
            };

            if (!ModelState.IsValid)
                return View(model);

            //if (PromoteToHomePage)
            //    CurrentSite.HomePage = "BlogHomePageProvider;" + model.Blog.Item.Id;

            _blogService.Edit(model.Blog.Item);

            Services.Notifier.Information(T("Blog information updated"));

            return Redirect(Url.BlogsForAdmin());
        }

        [HttpPost]
        public ActionResult Remove(string blogSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't delete blog")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            BlogPart blogPart = _blogService.Get(blogSlug);

            if (blogPart == null)
                return new NotFoundResult();

            _blogService.Delete(blogPart);

            Services.Notifier.Information(T("Blog was successfully deleted"));

            return Redirect(Url.BlogsForAdmin());
        }

        public ActionResult List() {
            //TODO: (erikpo) Need to make templatePath be more convention based so if my controller name has "Admin" in it then "Admin/{type}" is assumed
            var model = new AdminBlogsViewModel {
                Entries = _blogService.Get()
                    .Select(b => Services.ContentManager.BuildDisplayModel(b, "SummaryAdmin"))
                    .Select(vm => new AdminBlogEntry { ContentItemViewModel = vm, TotalPostCount = _blogPostService.Get(vm.Item, VersionOptions.Latest).Count()})
            };

            return View(model);
        }

        //TODO: (erikpo) Should move the slug parameter and get call and null check up into a model binder
        public ActionResult Item(string blogSlug) {
            BlogPart blogPart = _blogService.Get(blogSlug);

            if (blogPart == null)
                return new NotFoundResult();

            //TODO: (erikpo) Need to make templatePath be more convention based so if my controller name has "Admin" in it then "Admin/{type}" is assumed
            var model = new BlogForAdminViewModel {
                Blog = Services.ContentManager.BuildDisplayModel(blogPart, "DetailAdmin")
            };

            return View(model);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}