using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement;
using Orchard.Events;

namespace Orchard.Forms.Services {
    public interface IFormProvider : IEventHandler {
        void Describe(DescribeContext context);
    }

    public class DescribeContext {
        private readonly Dictionary<string, FormDescriptor> _descriptors = new Dictionary<string, FormDescriptor>();

        public IList<FormDescriptor> Describe() {
            return _descriptors.Select(x => x.Value).ToList();
        }

        public DescribeContext Form(string name, Func<IShapeFactory, dynamic> shape) {
            _descriptors[name] = new FormDescriptor { Name = name, Shape = shape };
            return this;
        }
    }

    public class FormDescriptor {
        public string Name { get; set; }
        public Func<IShapeFactory, dynamic> Shape { get; set; }
    }
}