using Orchard.ContentManagement;

namespace Orchard.Projections.Models {
    public class NavigationQueryPart : ContentPart<NavigationQueryPartRecord> {
        /// <summary>
        /// Maximum number of items to retrieve from db
        /// </summary>
        public virtual int Items {
            get { return Record.Items; }
            set { Record.Items = value; }
        }

        /// <summary>
        /// Number of items to skip
        /// </summary>
        public virtual int Skip {
            get { return Record.Skip; }
            set { Record.Skip = value; }
        }

        /// <summary>
        /// The query to execute
        /// </summary>
        public virtual QueryPartRecord QueryPartRecord {
            get { return Record.QueryPartRecord; }
            set { Record.QueryPartRecord = value; }
        }
    }
}