using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Layouts.Framework.Elements {
    public class LayoutSavingContext {
        public IUpdateModel Updater { get; set; }

        public IEnumerable<IElement> Elements { get; set; }
        public IEnumerable<IElement> RemovedElements { get; set; }
        public IContent Content { get; set; }
    }
}