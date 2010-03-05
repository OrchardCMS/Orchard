using System;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Blogs.Drivers;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
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

        public BlogAdminController(IOrchardServices services, IBlogService blogService, IBlogPostService blogPostService) {
            Services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Create() {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Not allowed to create blogs")))
                return new HttpUnauthorizedResult();

            var blog = Services.ContentManager.New<Blog>(BlogDriver.ContentType.Name);
            if (blog == null)
                return new NotFoundResult();

            var model = new CreateBlogViewModel {
                Blog = Services.ContentManager.BuildEditorModel(blog)
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateBlogViewModel model) {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't create blog")))
                return new HttpUnauthorizedResult();

            model.Blog = Services.ContentManager.UpdateEditorModel(Services.ContentManager.New<Blog>(BlogDriver.ContentType.Name), this);

            if (!ModelState.IsValid)
                return View(model);

            _blogService.Create(model.Blog.Item);

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
                Blog = Services.ContentManager.BuildEditorModel(blog)
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string blogSlug, FormCollection input) {
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

            string setAsHomePage = input["PromoteToHomePage"];
            if (!String.IsNullOrEmpty(setAsHomePage) && !setAsHomePage.Equals("false")) {
                CurrentSite.HomePage = "BlogHomePageProvider;" + model.Blog.Item.Id;
            }

            _blogService.Edit(model.Blog.Item);

            Services.Notifier.Information(T("Blog information updated"));

            return Redirect(Url.BlogsForAdmin());
        }

        [HttpPost]
        public ActionResult Delete(string blogSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't delete blog")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            _blogService.Delete(blog);

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
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            //TODO: (erikpo) Need to make templatePath be more convention based so if my controller name has "Admin" in it then "Admin/{type}" is assumed
            var model = new BlogForAdminViewModel {
                Blog = Services.ContentManager.BuildDisplayModel(blog, "DetailAdmin")
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