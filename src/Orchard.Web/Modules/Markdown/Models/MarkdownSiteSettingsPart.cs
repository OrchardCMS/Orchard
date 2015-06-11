using Orchard.ContentManagement;

namespace Markdown.Models {
    public class MarkdownSiteSettingsPart : ContentPart {

        public bool UseMarkdownForBlogs {
            get { return this.Retrieve(x => x.UseMarkdownForBlogs); }
            set { this.Store(x => x.UseMarkdownForBlogs, value); }
        }
    }
}