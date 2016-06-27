using Orchard.ContentManagement;

namespace Orchard.ContentPermissions.Models {
    public class ContentPermissionsPart : ContentPart {
        /// <summary>
        /// Whether the access control should be applied for the content item
        /// </summary>
        public bool Enabled {
            get { return this.Retrieve(x => x.Enabled); }
            set { this.Store(x => x.Enabled, value); }
        }

        public string ViewContent {
            get { return this.Retrieve(x => x.ViewContent); }
            set { this.Store(x => x.ViewContent, value); }
        }

        public string ViewOwnContent {
            get { return this.Retrieve(x => x.ViewOwnContent); }
            set { this.Store(x => x.ViewOwnContent, value); }
        }

        public string PublishContent {
            get { return this.Retrieve(x => x.PublishContent); }
            set { this.Store(x => x.PublishContent, value); }
        }

        public string PublishOwnContent {
            get { return this.Retrieve(x => x.PublishOwnContent); }
            set { this.Store(x => x.PublishOwnContent, value); }
        }

        public string EditContent {
            get { return this.Retrieve(x => x.EditContent); }
            set { this.Store(x => x.EditContent, value); }
        }

        public string EditOwnContent {
            get { return this.Retrieve(x => x.EditOwnContent); }
            set { this.Store(x => x.EditOwnContent, value); }
        }

        public string DeleteContent {
            get { return this.Retrieve(x => x.DeleteContent); }
            set { this.Store(x => x.DeleteContent, value); }
        }

        public string DeleteOwnContent {
            get { return this.Retrieve(x => x.DeleteOwnContent); }
            set { this.Store(x => x.DeleteOwnContent, value); }
        }

        public string PreviewContent {
            get { return this.Retrieve(x => x.PreviewContent); }
            set { this.Store(x => x.PreviewContent, value); }
        }

        public string PreviewOwnContent {
            get { return this.Retrieve(x => x.PreviewOwnContent); }
            set { this.Store(x => x.PreviewOwnContent, value); }
        }
    }
}
