using System.Collections;
using System.Collections.Generic;

namespace Orchard.Layouts.Framework.Drivers {
    public class EditorResult {
        public EditorResult() {
            Editors = new List<dynamic>();
        }

        public IList<dynamic> Editors { get; set; }

        public EditorResult Add(dynamic editor) {
            ((IList)Editors).Add(editor);
            return this;
        }
    }
}