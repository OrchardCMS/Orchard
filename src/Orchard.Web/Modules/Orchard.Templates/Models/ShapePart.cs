using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Templates.Models {
    public class ShapePart : ContentPart<ShapePartRecord> {
        public string Name {
            get { return Retrieve(x => x.Name); }
            set { Store(x => x.Name, value); }
        }

        public string Language {
            get { return Retrieve(x => x.Language); }
            set { Store(x => x.Language, value); }
        }

        public string Template {
            get { return Retrieve(x => x.Template); }
            set { Store(x => x.Template, value); }
        }
    }

    public class ShapePartRecord : ContentPartRecord {
        public virtual string Name { get; set; }
        public virtual string Language { get; set; }

        [StringLengthMax]
        public virtual string Template { get; set; }
    }
}