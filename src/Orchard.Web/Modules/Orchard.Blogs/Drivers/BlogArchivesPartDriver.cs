using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Blogs.Drivers {
    public class BlogArchivesPartDriver : ContentPartDriver<BlogArchivesPart> {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IContentManager _contentManager;

        public BlogArchivesPartDriver(
            IBlogService blogService, 
            IBlogPostService blogPostService,
            IContentManager contentManager) {
            _blogService = blogService;
            _blogPostService = blogPostService;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(BlogArchivesPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_BlogArchives",
                                () => {
                                    var blog = _blogService.Get(part.BlogId, VersionOptions.Published).As<BlogPart>();

                                    if (blog == null)
                                        return null;

                                    return shapeHelper.Parts_Blogs_BlogArchives(Blog: blog, Archives: _blogPostService.GetArchives(blog));
                                });
        }

        protected override DriverResult Editor(BlogArchivesPart part, dynamic shapeHelper) {
            var viewModel = new BlogArchivesViewModel {
                BlogId = part.BlogId,
                Blogs = _blogService.Get().ToList().OrderBy(b => b.Name)
                };

            return ContentShape("Parts_Blogs_BlogArchives_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.BlogArchives", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(BlogArchivesPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new BlogArchivesViewModel();
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                part.BlogId = viewModel.BlogId;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(BlogArchivesPart part, ImportContentContext context) {
            var blog = context.Attribute(part.PartDefinition.Name, "Blog");
            if (blog != null) {
                part.BlogId = context.GetItemFromSession(blog).Id;
            }
        }

        protected override void Exporting(BlogArchivesPart part, ExportContentContext context) {
            var blog = _contentManager.Get(part.BlogId);
            var blogIdentity = _contentManager.GetItemMetadata(blog).Identity;
            context.Element(part.PartDefinition.Name).SetAttributeValue("Blog", blogIdentity);
        }

    }
}