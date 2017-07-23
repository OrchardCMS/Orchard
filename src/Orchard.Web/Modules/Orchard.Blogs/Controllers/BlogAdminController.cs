using System.Linq;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Settings;

namespace Orchard.Blogs.Controllers {

    [ValidateInput(false), Admin]
    public class BlogAdminController : Controller, IUpdateModel {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly ISiteService _siteService;

        public BlogAdminController(
            IOrchardServices services,
            IBlogService blogService,
            IBlogPostService blogPostService,
            IContentManager contentManager,
            ITransactionManager transactionManager,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            Services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _siteService = siteService;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Not allowed to create blogs")))
                return new HttpUnauthorizedResult();

            BlogPart blog = Services.ContentManager.New<BlogPart>("Blog");
            if (blog == null)
                return HttpNotFound();

            var model = Services.ContentManager.BuildEditor(blog);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't create blog")))
                return new HttpUnauthorizedResult();

            var blog = Services.ContentManager.New<BlogPart>("Blog");

            _contentManager.Create(blog, VersionOptions.Draft);
            var model = _contentManager.UpdateEditor(blog, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(blog.ContentItem);
            return Redirect(Url.BlogForAdmin(blog));
        }

        public ActionResult Edit(int blogId) {
            var blog = _blogService.Get(blogId, VersionOptions.Latest);

            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, blog, T("Not allowed to edit blog")))
                return new HttpUnauthorizedResult();

            if (blog == null)
                return HttpNotFound();

            var model = Services.ContentManager.BuildEditor(blog);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditDeletePOST(int blogId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't delete blog")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogId, VersionOptions.DraftRequired);
            if (blog == null)
                return HttpNotFound();
            _blogService.Delete(blog);

            Services.Notifier.Success(T("Blog was deleted."));

            return Redirect(Url.BlogsForAdmin());
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public ActionResult EditPOST(int blogId) {
            var blog = _blogService.Get(blogId, VersionOptions.DraftRequired);

            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, blog, T("Couldn't edit blog")))
                return new HttpUnauthorizedResult();

            if (blog == null)
                return HttpNotFound();

            var model = Services.ContentManager.UpdateEditor(blog, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(blog);
            Services.Notifier.Success(T("Blog properties were updated."));

            return Redirect(Url.BlogsForAdmin());
        }

        [HttpPost]
        public ActionResult Remove(int blogId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't delete blog")))
                return new HttpUnauthorizedResult();

            var blog = _blogService.Get(blogId, VersionOptions.Latest);

            if (blog == null)
                return HttpNotFound();

            _blogService.Delete(blog);

            Services.Notifier.Success(T("Blog was successfully deleted."));
            return Redirect(Url.BlogsForAdmin());
        }

        public ActionResult List() {
            var list = Services.New.List();
            list.AddRange(_blogService.Get(VersionOptions.Latest)
                .Where(x => Services.Authorizer.Authorize(Permissions.MetaListOwnBlogs, x))
                .Select(b => {
                            var blog = Services.ContentManager.BuildDisplay(b, "SummaryAdmin");
                            blog.TotalPostCount = _blogPostService.PostCount(b, VersionOptions.Latest);
                            return blog;
                        }));

            var viewModel = Services.New.ViewModel()
                .ContentItems(list);
            return View(viewModel);
        }

        public ActionResult Item(int blogId, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            BlogPart blogPart = _blogService.Get(blogId, VersionOptions.Latest).As<BlogPart>();

            if (blogPart == null)
                return HttpNotFound();

            var blogPosts = _blogPostService.Get(blogPart, pager.GetStartIndex(), pager.PageSize, VersionOptions.Latest).ToArray();
            var blogPostsShapes = blogPosts.Select(bp => _contentManager.BuildDisplay(bp, "SummaryAdmin")).ToArray();

            var blog = Services.ContentManager.BuildDisplay(blogPart, "DetailAdmin");

            var list = Shape.List();
            list.AddRange(blogPostsShapes);
            blog.Content.Add(Shape.Parts_Blogs_BlogPost_ListAdmin(ContentItems: list), "5");

            var totalItemCount = _blogPostService.PostCount(blogPart, VersionOptions.Latest);
            blog.Content.Add(Shape.Pager(pager).TotalItemCount(totalItemCount), "Content:after");

            return View(blog);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}