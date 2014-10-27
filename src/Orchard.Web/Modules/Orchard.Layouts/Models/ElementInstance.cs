//using System.Collections.Generic;
//using Orchard.Layouts.Framework.Elements;

//namespace Orchard.Layouts.Models {
//    public class ElementInstance {

//        public ElementInstance(string id, ElementDescriptor elementDescriptor, int index = 0, IDictionary<string, string> state = null) {
//            Id = id;
//            ElementDescriptor = elementDescriptor;
//            State = state ?? new Dictionary<string, string>();
//            Children = new List<ElementInstance>();
//            Index = index;
//        }

//        public string Id { get; set; }

//        public ElementDescriptor ElementDescriptor { get; set; }
//        public ElementInstance Parent { get; set; }
//        public IList<ElementInstance> Children { get; set; }
//        public int Index { get; set; }
//        public IDictionary<string, string> State { get; set; }
//        public bool IsTemplated { get; set; }
//    }
//}