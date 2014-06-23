using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
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

        public async Task<ActionResult> Create() {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Not allowed to create blogs")))
                return new HttpUnauthorizedResult();

            BlogPart blog = Services.ContentManager.New<BlogPart>("Blog");
            if (blog == null)
                return HttpNotFound();

            var model = await Services.ContentManager.BuildEditorAsync(blog);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public async Task<ActionResult> CreatePOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't create blog")))
                return new HttpUnauthorizedResult();

            var blog = Services.ContentManager.New<BlogPart>("Blog");

            _contentManager.Create(blog, VersionOptions.Draft);
            var model = await _contentManager.UpdateEditorAsync(blog, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(blog.ContentItem);
            return Redirect(Url.BlogForAdmin(blog));
        }

        public async Task<ActionResult> Edit(int blogId) {
            var blog = _blogService.Get(blogId, VersionOptions.Latest);

            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, blog, T("Not allowed to edit blog")))
                return new HttpUnauthorizedResult();

            if (blog == null)
                return HttpNotFound();

            var model = await Services.ContentManager.BuildEditorAsync(blog);
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

            Services.Notifier.Information(T("Blog deleted"));

            return Redirect(Url.BlogsForAdmin());
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditPOST(int blogId) {
            var blog = _blogService.Get(blogId, VersionOptions.DraftRequired);

            if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, blog, T("Couldn't edit blog")))
                return new HttpUnauthorizedResult();

            if (blog == null)
                return HttpNotFound();

            var model = await Services.ContentManager.UpdateEditorAsync(blog, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(blog);
            Services.Notifier.Information(T("Blog information updated"));

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

            Services.Notifier.Information(T("Blog was successfully deleted"));
            return Redirect(Url.BlogsForAdmin());
        }

        public async Task<ActionResult> List() {
            var list = Services.New.List();

            var blogs = _blogService.Get(VersionOptions.Latest)
                .Where(x => Services.Authorizer.Authorize(Permissions.MetaListOwnBlogs, x));

            var shapeTasks = blogs.Select(async b => {
                var blog = await Services.ContentManager.BuildDisplayAsync(b, "SummaryAdmin");
                blog.TotalPostCount = _blogPostService.PostCount(b, VersionOptions.Latest);
                return blog;
            }).ToArray();

            await Task.WhenAll(shapeTasks);

            list.AddRange(shapeTasks.Select(task => task.Result));

            var viewModel = Services.New.ViewModel()
                .ContentItems(list);
            return View(viewModel);
        }

        public async Task<ActionResult> Item(int blogId, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            BlogPart blogPart = _blogService.Get(blogId, VersionOptions.Latest).As<BlogPart>();

            if (blogPart == null)
                return HttpNotFound();

            var blogPosts = _blogPostService.Get(blogPart, pager.GetStartIndex(), pager.PageSize, VersionOptions.Latest).ToArray();
            var blogPostsShapeTasks = blogPosts.Select(bp => _contentManager.BuildDisplayAsync(bp, "SummaryAdmin")).ToArray();
            var list = Shape.List();
            var blogTask = Services.ContentManager.BuildDisplayAsync(blogPart, "DetailAdmin");

            await Task.WhenAll(blogPostsShapeTasks.Cast<Task>().Concat(new Task[] { blogTask }));

            list.AddRange(blogPostsShapeTasks.Select(task => task.Result));
            var blog = blogTask.Result;
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