using System.Collections;
using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Drivers {
    public class EditorResult {
        public EditorResult() {
            Editors = new List<dynamic>();
            State = new StateDictionary();
        }

        public IList<dynamic> Editors { get; set; }
        public StateDictionary State { get; set; }

        public EditorResult Add(dynamic editor) {
            ((IList)Editors).Add(editor);
            return this;
        }
    }
}