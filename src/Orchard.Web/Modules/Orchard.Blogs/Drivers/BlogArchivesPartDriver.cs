using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Blogs.Drivers {
    public class BlogArchivesPartDriver : ContentPartDriver<BlogArchivesPart> {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogArchivesPartDriver(IBlogService blogService, IBlogPostService blogPostService) {
            _blogService = blogService;
            _blogPostService = blogPostService;
        }

        protected override DriverResult Display(BlogArchivesPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_BlogArchives",
                                () => {
                                    BlogPart blog = null;
                                    if (!string.IsNullOrWhiteSpace(part.ForBlog))
                                        blog = _blogService.Get(part.ForBlog);

                                    if (blog == null)
                                        return null;

                                    return shapeHelper.Parts_Blogs_BlogArchives(ContentItem: part.ContentItem, Blog: blog, Archives: _blogPostService.GetArchives(blog));
                                });
        }

        protected override DriverResult Editor(BlogArchivesPart part, dynamic shapeHelper) {
            var viewModel = new BlogArchivesViewModel {
                Slug = part.ForBlog,
                Blogs = _blogService.Get().ToList().OrderBy(b => b.Name)
                };

            return ContentShape("Parts_Blogs_BlogArchives_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.BlogArchives", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(BlogArchivesPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new BlogArchivesViewModel();
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                part.ForBlog = viewModel.Slug;
            }

            return Editor(part, shapeHelper);
        }
    }
}