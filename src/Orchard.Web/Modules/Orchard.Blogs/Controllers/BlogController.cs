using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Services;
using Orchard.Core.Feeds;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.Settings;
using Orchard.ContentManagement;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Controllers {

    [Themed]
    public class BlogController : Controller {
        private readonly IOrchardServices _services;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IFeedManager _feedManager;
        private readonly ISiteService _siteService;

        public BlogController(
            IOrchardServices services, 
            IBlogService blogService,
            IBlogPostService blogPostService,
            IFeedManager feedManager, 
            IShapeFactory shapeFactory,
            ISiteService siteService) {
            _services = services;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _feedManager = feedManager;
            _siteService = siteService;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        protected ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public async Task<ActionResult> List() {
            var blogTasks = _blogService.Get()
                .Where(b => _services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent,b))
                .Select(b => _services.ContentManager.BuildDisplayAsync(b, "Summary")).ToArray();

            var list = Shape.List();

            await Task.WhenAll(blogTasks);

            list.AddRange(blogTasks.Select(task => task.Result));

            var viewModel = Shape.ViewModel()
                .ContentItems(list);

            return View(viewModel);
        }

        public async Task<ActionResult> Item(int blogId, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var blogPart = _blogService.Get(blogId, VersionOptions.Published).As<BlogPart>();
            if (blogPart == null)
                return HttpNotFound();

            if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, blogPart, T("Cannot view content"))) {
                return new HttpUnauthorizedResult();
            }

            _feedManager.Register(blogPart, _services.ContentManager.GetItemMetadata(blogPart).DisplayText);
            var blogPostTasks = _blogPostService.Get(blogPart, pager.GetStartIndex(), pager.PageSize)
                .Select(b => _services.ContentManager.BuildDisplayAsync(b, "Summary")).ToArray();
            var blogTask = _services.ContentManager.BuildDisplayAsync(blogPart);

            await Task.WhenAll(blogPostTasks.Cast<Task>().Concat(new Task[] { blogTask }));

            var list = Shape.List();
            list.AddRange(blogPostTasks.Select(task => task.Result));
            var blog = blogTask.Result;
            blog.Content.Add(Shape.Parts_Blogs_BlogPost_List(ContentItems: list), "5");

            var totalItemCount = _blogPostService.PostCount(blogPart);
            blog.Content.Add(Shape.Pager(pager).TotalItemCount(totalItemCount), "Content:after");

            return new ShapeResult(this, blog);
        }
    }
}
