using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Blogs.Models {
    public class BlogPart : ContentPart {

        public string Name {
            get { return this.As<ITitleAspect>().Title; }
        }

        public string Description {
            get { return this.Retrieve(x => x.Description); }
            set { this.Store(x => x.Description, value); }
        }

        public int PostCount {
            get { return this.Retrieve(x => x.PostCount); }
            set { this.Store(x => x.PostCount, value); }
        }

        public string FeedProxyUrl {
            get { return this.Retrieve(x => x.FeedProxyUrl); }
            set { this.Store(x => x.FeedProxyUrl, value); }
        }

        public bool EnableCommentsFeed {
            get { return this.Retrieve(x => x.EnableCommentsFeed, false); }
            set { this.Store(x => x.EnableCommentsFeed, value); }
        }

        public int PostsPerPage {
            get { return this.Retrieve(x => x.PostsPerPage, 10); }
            set { this.Store(x => x.PostsPerPage, value); }
        }
    }
}