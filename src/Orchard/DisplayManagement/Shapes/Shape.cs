using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WebPages;

namespace Orchard.DisplayManagement.Shapes {
    public class Shape : IShape, IEnumerable {
        public virtual ShapeMetadata Metadata { get; set; }

        private readonly IList<object> _items = new List<object>();


        public virtual Shape Add(object item) {
            _items.Add(item);
            return this;
        }

        public virtual Shape Add(object item, string position) {
            try {
                ((dynamic)item).Metadata.Position = position;
            }
            catch {
                // need to implemented positioned wrapper for non-shape objects
            }
            _items.Add(item); // not messing with position at the moment
            return this;
        }

        public virtual IEnumerator GetEnumerator() {
            return _items.GetEnumerator();
        }
     
        public virtual IEnumerable<dynamic> Items {
            get { return _items; }
        }
    }
}
