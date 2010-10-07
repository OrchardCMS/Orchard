using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;

namespace Orchard.Blogs.Filters {
    public class ArchivesFilter : FilterProvider, IResultFilter {
        private readonly IBlogPostService _blogPostService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IShapeHelperFactory _shapeHelperFactory;

        public ArchivesFilter(IBlogPostService blogPostService, IWorkContextAccessor workContextAccessor, IShapeHelperFactory shapeHelperFactory) {
            _blogPostService = blogPostService;
            _workContextAccessor = workContextAccessor;
            _shapeHelperFactory = shapeHelperFactory;
        }

        //todo: make work for real
        public void OnResultExecuting(ResultExecutingContext filterContext) {
            dynamic model = filterContext.Controller.ViewData.Model;

            if (model == null || model.GetType().GetProperty("Metadata") == null || model.Metadata.GetType().GetProperty("Type") == null)
                return;

            var modelType = model.Metadata.Type;
            if (string.IsNullOrEmpty(modelType))
                return;

            var workContext = _workContextAccessor.GetContext(filterContext);
            var shape = _shapeHelperFactory.CreateHelper();

            if (modelType == "Items_Content_Blog") {
                BlogPart blog = model.ContentItem.Get(typeof (BlogPart));
                var blogArchives = shape.BlogArchives()
                    .Archives(new BlogPostArchiveViewModel { BlogPart = blog, Archives = _blogPostService.GetArchives(blog) });
                workContext.Layout.Sidebar.Add(blogArchives);
                return;
            }

            if (modelType == "Items_Content_BlogPost" || modelType == "BlogPostArchive") {
                BlogPart blog = model.Blog.ContentItem.Get(typeof (BlogPart));
                var blogArchives = shape.BlogArchives()
                    .Archives(new BlogPostArchiveViewModel { BlogPart = blog, Archives = _blogPostService.GetArchives(blog) });
                workContext.Layout.Sidebar.Add(blogArchives);
                return;
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}