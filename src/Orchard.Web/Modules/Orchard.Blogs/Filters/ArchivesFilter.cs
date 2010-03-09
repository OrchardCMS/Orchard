using System.Web.Mvc;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.Mvc.Filters;

namespace Orchard.Blogs.Filters {
    public class ArchivesFilter : FilterProvider, IResultFilter {
        private readonly IBlogPostService _blogPostService;

        public ArchivesFilter(IBlogPostService blogPostService) {
            _blogPostService = blogPostService;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {

            var blogViewModel = filterContext.Controller.ViewData.Model as BlogViewModel;
            if (blogViewModel != null) {
                blogViewModel.Zones.AddRenderPartial("sidebar", "Archives", new BlogArchivesViewModel { Blog = blogViewModel.Blog.Item, Archives = _blogPostService.GetArchives(blogViewModel.Blog.Item) });
                return;
            }

            var blogPostViewModel = filterContext.Controller.ViewData.Model as BlogPostViewModel;
            if (blogPostViewModel != null) {
                blogPostViewModel.Zones.AddRenderPartial("sidebar", "Archives", new BlogArchivesViewModel { Blog = blogPostViewModel.Blog, Archives = _blogPostService.GetArchives(blogPostViewModel.Blog) });
                return;
            }

            var blogPostArchiveViewModel = filterContext.Controller.ViewData.Model as BlogPostArchiveViewModel;
            if (blogPostArchiveViewModel != null) {
                blogPostArchiveViewModel.Zones.AddRenderPartial("sidebar", "Archives", new BlogArchivesViewModel { Blog = blogPostArchiveViewModel.Blog, Archives = _blogPostService.GetArchives(blogPostArchiveViewModel.Blog) });
                return;
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}