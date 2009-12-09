using System.Collections.Generic;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Blogs.Models {
    public class BlogHandler : ContentHandler {
        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] { Blog.ContentType };
        }

        public BlogHandler(IRepository<BlogRecord> repository) {
            Filters.Add(new ActivatingFilter<Blog>("blog"));
            Filters.Add(new ActivatingFilter<CommonAspect>("blog"));
            Filters.Add(new ActivatingFilter<RoutableAspect>("blog"));
            Filters.Add(new StorageFilter<BlogRecord>(repository));
            Filters.Add(new ContentItemTemplates<Blog>("Blog", "Detail", "DetailAdmin", "Summary", "SummaryAdmin"));

            OnGetEditorViewModel<Blog>((context, blog) =>
                context.AddEditor(new TemplateViewModel(blog) {TemplateName = "BlogFields"})
            );

            OnUpdateEditorViewModel<Blog>((context, blog) => {
                context.AddEditor(new TemplateViewModel(blog) {TemplateName = "BlogFields"});
                context.Updater.TryUpdateModel(blog, "", null, null);
            });
        }
    }
}