using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentTypes.ViewModels {
    public class AddFieldViewModel {
        public AddFieldViewModel() {
            Fields = new List<ContentFieldInfo>();
        }

        /// <summary>
        /// The technical name of the field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The display name of the field
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The selected field type
        /// </summary>
        public string FieldTypeName { get; set; }

        /// <summary>
        /// The part to add the field to
        /// </summary>
        public EditPartViewModel Part { get; set; }

        /// <summary>
        /// List of the available Field types
        /// </summary>
        public IEnumerable<ContentFieldInfo> Fields { get; set; }
    }
}