using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Blogs.Drivers {
    public class BlogArchivesPartDriver : ContentPartDriver<BlogArchivesPart> {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IBlogPathConstraint _blogPathConstraint;

        public BlogArchivesPartDriver(
            IBlogService blogService, 
            IBlogPostService blogPostService,
            IBlogPathConstraint blogPathConstraint) {
            _blogService = blogService;
            _blogPostService = blogPostService;
            _blogPathConstraint = blogPathConstraint;
        }

        protected override DriverResult Display(BlogArchivesPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_BlogArchives",
                                () => {
                                    var path = _blogPathConstraint.FindPath(part.ForBlog);
                                    BlogPart blog = _blogService.Get(path);

                                    if (blog == null)
                                        return null;

                                    return shapeHelper.Parts_Blogs_BlogArchives(ContentItem: part.ContentItem, Blog: blog, Archives: _blogPostService.GetArchives(blog));
                                });
        }

        protected override DriverResult Editor(BlogArchivesPart part, dynamic shapeHelper) {
            var viewModel = new BlogArchivesViewModel {
                Path = part.ForBlog,
                Blogs = _blogService.Get().ToList().OrderBy(b => b.Name)
                };

            return ContentShape("Parts_Blogs_BlogArchives_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.BlogArchives", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(BlogArchivesPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new BlogArchivesViewModel();
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                part.ForBlog = viewModel.Path;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(BlogArchivesPart part, ImportContentContext context) {
            var blogSlug = context.Attribute(part.PartDefinition.Name, "BlogSlug");
            if (blogSlug != null) {
                part.ForBlog = blogSlug;
            }
        }

        protected override void Exporting(BlogArchivesPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("BlogSlug", part.ForBlog);
        }
    }
}