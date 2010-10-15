using Orchard.Blogs.Models;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Blogs.Drivers {
    public class BlogPagerPartDriver : ContentPartDriver<BlogPagerPart> {
        protected override DriverResult Display(BlogPagerPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_Blog_Pager",
                                () => shapeHelper.Parts_Blogs_Blog_Pager(ContentPart: part, Page: part.Page, PageSize: part.PageSize, BlogSlug: part.BlogSlug, ThereIsANextPage: part.ThereIsANextPage));
        }
    }
}