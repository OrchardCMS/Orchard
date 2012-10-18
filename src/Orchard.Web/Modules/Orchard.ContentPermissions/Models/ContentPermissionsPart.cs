using Orchard.ContentManagement;

namespace Orchard.ContentPermissions.Models {
    public class ContentPermissionsPart : ContentPart<ContentPermissionsPartRecord> {
        /// <summary>
        /// Whether the access control should be applied for the content item
        /// </summary>
        public bool Enabled {
            get { return Record.Enabled; }
            set { Record.Enabled = value; }
        }

        public string ViewContent {
            get { return Record.ViewContent; }
            set { Record.ViewContent = value; }
        }

        public string ViewOwnContent {
            get { return Record.ViewOwnContent; }
            set { Record.ViewOwnContent = value; }
        }

        public string PublishContent {
            get { return Record.PublishContent; }
            set { Record.PublishContent = value; }
        }

        public string PublishOwnContent {
            get { return Record.PublishOwnContent; }
            set { Record.PublishOwnContent = value; }
        }

        public string EditContent {
            get { return Record.EditContent; }
            set { Record.EditContent = value; }
        }

        public string EditOwnContent {
            get { return Record.EditOwnContent; }
            set { Record.EditOwnContent = value; }
        }

        public string DeleteContent {
            get { return Record.DeleteContent; }
            set { Record.DeleteContent = value; }
        }

        public string DeleteOwnContent {
            get { return Record.DeleteOwnContent; }
            set { Record.DeleteOwnContent = value; }
        }
    }
}
