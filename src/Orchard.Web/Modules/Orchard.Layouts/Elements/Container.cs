using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Elements {
    public abstract class Container : Element, IContainer {
        protected Container() {
            Elements = new List<IElement>();
        }
        public IList<IElement> Elements { get; set; }
        public int Index { get; set; }
    }
}