using System;

namespace Orchard.DynamicForms.Services.Models {
    public abstract class BindingDescriptor {
        public string Name { get; set; }
        public Delegate Setter { get; set; }
    }

    public class BindingDescriptor<T> : BindingDescriptor {
    }
}