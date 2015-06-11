using Orchard.ContentManagement.Records;

namespace Orchard.Projections.Models {
    public class NavigationQueryPartRecord : ContentPartRecord {
        /// <summary>
        /// Maximum number of items to retrieve from db
        /// </summary>
        public virtual int Items { get; set; }

        /// <summary>
        /// Number of items to skip
        /// </summary>
        public virtual int Skip { get; set; }

        /// <summary>
        /// The query to execute
        /// </summary>
        public virtual QueryPartRecord QueryPartRecord { get; set; }
    }
}