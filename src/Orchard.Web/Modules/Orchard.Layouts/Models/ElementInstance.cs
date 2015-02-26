//using System.Collections.Generic;
//using Orchard.Layouts.Framework.Elements;

//namespace Orchard.Layouts.Models {
//    public class ElementInstance {

//        public ElementInstance(string id, ElementDescriptor elementDescriptor, int index = 0, IDictionary<string, string> Data = null) {
//            Id = id;
//            ElementDescriptor = elementDescriptor;
//            Data = Data ?? new Dictionary<string, string>();
//            Children = new List<ElementInstance>();
//            Index = index;
//        }

//        public string Id { get; set; }

//        public ElementDescriptor ElementDescriptor { get; set; }
//        public ElementInstance Parent { get; set; }
//        public IList<ElementInstance> Children { get; set; }
//        public int Index { get; set; }
//        public IDictionary<string, string> Data { get; set; }
//        public bool IsTemplated { get; set; }
//    }
//}