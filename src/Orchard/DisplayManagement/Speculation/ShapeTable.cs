using System;
using System.Collections.Generic;
using System.Web;

namespace Orchard.DisplayManagement {
    public class ShapeTable {
        public IDictionary<string, Entry> Entries { get; set; }

        public class Entry {
            public string ShapeType { get; set; }
            public Func<DisplayContext, object> Target { get; set; }
        }
    }
}
