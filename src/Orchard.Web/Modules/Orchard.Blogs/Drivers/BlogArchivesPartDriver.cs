using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Blogs.Drivers {
    public class BlogArchivesPartDriver : ContentPartDriver<BlogArchivesPart> {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogArchivesPartDriver(IBlogService blogService, IBlogPostService blogPostService) {
            _blogService = blogService;
            _blogPostService = blogPostService;
        }

        protected override DriverResult Display(BlogArchivesPart part, string displayType, dynamic shapeHelper) {
            BlogPart blog = null;
            if (!string.IsNullOrWhiteSpace(part.ForBlog))
                blog = _blogService.Get(part.ForBlog);

            if ( blog != null ) {
                var model = new BlogPostArchiveViewModel {BlogPart = blog, Archives = _blogPostService.GetArchives(blog)};
                return ContentPartTemplate(model, "Parts/Blogs.BlogArchives");
            }

            return null;
        }

        protected override DriverResult Editor(BlogArchivesPart part, dynamic shapeHelper) {
            var location = part.GetLocation("Editor", "Primary", "5");
            return ContentPartTemplate(part, "Parts/Blogs.BlogArchives").Location(location);
        }

        protected override DriverResult Editor(BlogArchivesPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}