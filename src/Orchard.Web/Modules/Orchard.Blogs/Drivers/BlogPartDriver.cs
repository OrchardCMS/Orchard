using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]
    public class BlogPartDriver : ContentPartDriver<BlogPart> {
        protected override DriverResult Display(BlogPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Blogs_Blog_Manage",
                             () => shapeHelper.Parts_Blogs_Blog_Manage(ContentPart: part)),
                ContentShape("Parts_Blogs_Blog_Description",
                             () => shapeHelper.Parts_Blogs_Blog_Description(ContentPart: part, Description: part.Description)),
                ContentShape("Parts_Blogs_Blog_SummaryAdmin",
                             () => shapeHelper.Parts_Blogs_Blog_SummaryAdmin(ContentPart: part, ContentItem: part.ContentItem)),
                ContentShape("Parts_Blogs_Blog_BlogPostCount",
                             () => shapeHelper.Parts_Blogs_Blog_BlogPostCount(ContentPart: part, PostCount: part.PostCount))
                );
        }

        protected override DriverResult Editor(BlogPart blogPart, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_Blog_Fields",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.Blog.Fields", Model: blogPart, Prefix: Prefix));
        }

        protected override DriverResult Editor(BlogPart blogPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(blogPart, Prefix, null, null);
            return Editor(blogPart, shapeHelper);
        }
    }
}