using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.ContentManagement.Records {
    public class ContentItemRecord {
        public ContentItemRecord() {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Versions = new List<ContentItemVersionRecord>();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
            Infoset = new Infoset();
        }

        public virtual int Id { get; set; }
        public virtual ContentTypeRecord ContentType { get; set; }
        public virtual IList<ContentItemVersionRecord> Versions { get; set; }

        public virtual string Data {
            get { return Infoset.Data; }
            set { Infoset.Data = value; }
        }

        public virtual Infoset Infoset { get; private set; }
    }

    public class Infoset {
        private XElement _element;

        private void SetElement(XElement value) {
            _element = value;
        }

        public XElement Element {
            get {
                return _element ?? (_element = new XElement("Data"));
            }
        }

        public string Data {
            get { return Element.ToString(SaveOptions.DisableFormatting); }
            set { SetElement(XElement.Parse(value, LoadOptions.PreserveWhitespace)); }
        }
    }
}
