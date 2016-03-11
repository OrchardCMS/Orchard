using System;

﻿namespace Orchard.Layouts.Models {
﻿        [Serializable]
        public class ElementSessionState {
        public string TypeName { get; set; }
        public string ElementData { get; set; }
        public string ElementEditorData { get; set; }
        public int? ContentId { get; set; }
        public string ContentType { get; set; }
    }
}
