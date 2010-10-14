using System.Linq;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Results;
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
            IBlogSlugConstraint blogSlugConstraint,
            IShapeHelperFactory shapeHelperFactory) {
            Services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _blogSlugConstraint = blogSlugConstraint;
            T = NullLocalizer.Instance;
            Shape = shapeHelperFactory.CreateHelper();
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Create() {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Not allowed to create blogs")))
                return new HttpUnauthorizedResult();

            var blog = Services.ContentManager.New<BlogPart>("Blog");
            if (blog == null)
                return new NotFoundResult();

            var model = Services.ContentManager.BuildEditor(blog);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST() {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't create blog")))
                return new HttpUnauthorizedResult();

            var blog = Services.ContentManager.New<BlogPart>("Blog");

            _contentManager.Create(blog, VersionOptions.Draft);
            var model = _contentManager.UpdateEditor(blog, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            if (!blog.Has<IPublishingControlAspect>())
                _contentManager.Publish(blog.ContentItem);

            _blogSlugConstraint.AddSlug((string)model.Slug);
            return Redirect(Url.BlogForAdmin((string)model.Slug));
        }

        public ActionResult Edit(string blogSlug) {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Not allowed to edit blog")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var model = Services.ContentManager.BuildEditor(blog);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string blogSlug) {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't edit blog")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogSlug);
            if (blog == null)
                return new NotFoundResult();

            var model = Services.ContentManager.UpdateEditor(blog, this);
            if (!ModelState.IsValid)
                return View(model);

            _blogSlugConstraint.AddSlug(blog.Slug);
            Services.Notifier.Information(T("Blog information updated"));
            return Redirect(Url.BlogsForAdmin());
        }

        [HttpPost]
        public ActionResult Remove(string blogSlug) {
            //TODO: (erikpo) Might think about moving this to an ActionFilter/Attribute
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't delete blog")))
                return new HttpUnauthorizedResult();

            BlogPart blogPart = _blogService.Get(blogSlug);

            if (blogPart == null)
                return new NotFoundResult();

            _blogService.Delete(blogPart);

            Services.Notifier.Information(T("Blog was successfully deleted"));
            return Redirect(Url.BlogsForAdmin());
        }

        public ActionResult List() {
            var list = Shape.List();
            list.AddRange(_blogService.Get()
                              .Select(b => {
                                          var blog = Services.ContentManager.BuildDisplay(b, "SummaryAdmin");
                                          blog.TotalPostCount = _blogPostService.Get(b, VersionOptions.Latest).Count();
                                          return blog;
                                      }));

            var viewModel = Shape.ViewModel()
                .ContentItems(list);

            return View(viewModel);
        }

        public ActionResult Item(string blogSlug) {
            BlogPart blogPart = _blogService.Get(blogSlug);

            if (blogPart == null)
                return new NotFoundResult();

            var model = Services.ContentManager.BuildDisplay(blogPart, "DetailAdmin");
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