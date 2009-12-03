using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogController : Controller, IUpdateModel {
        private readonly ISessionLocator _sessionLocator;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogController(ISessionLocator sessionLocator, IContentManager contentManager, INotifier notifier, IBlogService blogService, IBlogPostService blogPostService) {
            _sessionLocator = sessionLocator;
            _contentManager = contentManager;
            _notifier = notifier;
            _blogService = blogService;
            _blogPostService = blogPostService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult List() {
            return View(new BlogsViewModel { Blogs = _blogService.Get() });
        }

        public ActionResult ListForAdmin() {
            return View(new BlogsForAdminViewModel { Blogs = _blogService.Get() });
        }

        //TODO: (erikpo) Should move the slug parameter and get call and null check up into a model binder
        public ActionResult Item(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            IEnumerable<BlogPost> posts = _blogPostService.Get(blog);

            return View(new BlogViewModel { Blog = blog, Posts = posts });
        }

        //TODO: (erikpo) Should move the slug parameter and get call and null check up into a model binder
        public ActionResult ItemForAdmin(string blogSlug)
        {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            IEnumerable<BlogPost> posts = _blogPostService.Get(blog);

            return View(new BlogForAdminViewModel { Blog = blog, Posts = posts });
        }

        public ActionResult Create() {
            return View(new CreateBlogViewModel());
        }

        [HttpPost]
        public ActionResult Create(CreateBlogViewModel model) {
            if (!ModelState.IsValid)
                return View(model);

            Blog blog = _blogService.Create(model.ToCreateBlogParams());

            //TEMP: (erikpo) ensure information has committed for this record
            var session = _sessionLocator.For(typeof(BlogRecord));
            session.Flush();

            //TODO: (erikpo) This should redirect to the blog homepage in the admin once that page is created
            return Redirect(Url.BlogsForAdmin());
        }

        public ActionResult Edit(string blogSlug) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var model = new BlogEditViewModel { Blog = blog };
            model.ItemView = _contentManager.GetEditors(model.Blog.ContentItem, "");
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string blogSlug, FormCollection input) {
            //TODO: (erikpo) Move looking up the current blog up into a modelbinder
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            var model = new BlogEditViewModel { Blog = blog };
            model.ItemView = _contentManager.UpdateEditors(model.Blog.ContentItem, "",this);

            IValueProvider values = input.ToValueProvider();
            if (!TryUpdateModel(model, values))
                return View(model);

            _notifier.Information(T("Blog information updated"));

            //TODO: (erikpo) This should redirect to the blog homepage in the admin once that page is created
            return Redirect(Url.BlogsForAdmin());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
    }
}