using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Templates.Models {
    public class ShapePart : ContentPart<ShapePartRecord> {
        public string Name {
            get { return Record.Name; }
            set { Record.Name = value; }
        }

        public string Language {
            get { return Record.Language; }
            set { Record.Language = value; }
        }

        public string Template {
            get { return Record.Template; }
            set { Record.Template = value; }
        }
    }

    public class ShapePartRecord : ContentPartRecord {
        public virtual string Name { get; set; }
        public virtual string Language { get; set; }

        [StringLengthMax]
        public virtual string Template { get; set; }
    }
}