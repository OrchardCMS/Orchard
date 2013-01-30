using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Contrib.Taxonomies.Models {
    public class TermWidgetPart : ContentPart<TermWidgetPartRecord> {
        /// <summary>
        /// The taxonomy to display
        /// </summary>

        public TaxonomyPartRecord TaxonomyPartRecord {
            get { return Record.TaxonomyPartRecord; }
            set { Record.TaxonomyPartRecord = value; }
        }

        public TermPartRecord TermPartRecord {
            get { return Record.TermPartRecord; }
            set { Record.TermPartRecord = value; }
        }

        [Range(1, int.MaxValue)]
        public int Count {
            get { return Record.Count; }
            set { Record.Count = value; }
        }

        public string OrderBy {
            get { return Record.OrderBy; }
            set { Record.OrderBy = value; }
        }

        public string FieldName {
            get { return Record.FieldName; }
            set { Record.FieldName = value;  }
        }

        public string ContentType {
            get { return Record.ContentType; }
            set { Record.ContentType = value; }
        }
    }
}