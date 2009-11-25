using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [ValidateInput(false)]
    public class BlogController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly ISessionLocator _sessionLocator;

        public BlogController(IContentManager contentManager, INotifier notifier, IBlogService blogService, IBlogPostService blogPostService, ISessionLocator sessionLocator) {
            _contentManager = contentManager;
            _notifier = notifier;
            _blogService = blogService;
            _blogPostService = blogPostService;
            _sessionLocator = sessionLocator;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult List() {
            return View(new BlogsViewModel {Blogs = _blogService.Get()});
        }

        //TODO: (erikpo) Should move the slug parameter and get call and null check up into a model binder
        public ActionResult Item(string blogSlug) {
            Blog blog = _blogService.Get(blogSlug);

            if (blog == null)
                return new NotFoundResult();

            IEnumerable<BlogPost> posts = _blogPostService.Get(blog);

            return View(new BlogViewModel {Blog = blog, Posts = posts});
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

            return Redirect(Url.BlogEdit(blog.As<RoutableAspect>().Slug));
        }

        public ActionResult Edit(string blogSlug) {
            var model = new BlogEditViewModel { Blog = _blogService.Get(blogSlug) };
            model.Editors = _contentManager.GetEditors(model.Blog.ContentItem);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string blogSlug, FormCollection input) {
            var model = new BlogEditViewModel { Blog = _blogService.Get(blogSlug) };
            model.Editors = _contentManager.UpdateEditors(model.Blog.ContentItem, this);

            IValueProvider values = input.ToValueProvider();
            if (!TryUpdateModel(model, values))
                return View(model);

            _notifier.Information(T("Blog information updated"));

            //TODO: (erikpo) Since the model isn't actually updated yet and it's possible the slug changed I'm getting the slug from input. Lame?!?!
            return Redirect(Url.BlogEdit(values.GetValue(ControllerContext, "Slug").RawValue as string));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
    }
}