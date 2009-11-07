using System;
using System.Web.Mvc;

namespace Orchard.Mvc {
    public class ModelBinderDescriptor {
        public Type Type { get; set; }
        public IModelBinder ModelBinder { get; set; }
    }
}