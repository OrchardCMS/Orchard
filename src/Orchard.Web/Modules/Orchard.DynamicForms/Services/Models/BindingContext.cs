using System;
using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.DynamicForms.Services.Models {
    public abstract class BindingContext {
        protected readonly IList<BindingDescriptor> _bindings = new List<BindingDescriptor>();

        protected BindingContext(string contextName) {
            ContextName = contextName;
        }

        public string ContextName { get; set; }
        public IEnumerable<BindingDescriptor> Bindings {
            get { return _bindings; }
        }
    }

    public class BindingContext<T> : BindingContext {

        public BindingContext() : base(typeof(T).Name) {
        }

        public BindingContext<T> Binding(string name, Action<ContentItem, T, string> setter) {

            _bindings.Add(new BindingDescriptor<T> {
                Name = name,
                Setter = setter
            });

            return this;
        }
    }
}