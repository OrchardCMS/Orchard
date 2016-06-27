using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Elements {
    public abstract class Container : Element {
        protected Container() {
            Elements = new List<Element>();
        }
        public IList<Element> Elements { get; set; }
    }
}