using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Handlers;
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

        public DescribeContext Form(string name, Func<IShapeFactory, dynamic> shape, Action<dynamic, ImportContentContext> importing, Action<dynamic, ExportContentContext> exporting) {
            _descriptors[name] = new FormDescriptor { Name = name, Shape = shape, Import = importing, Export = exporting};
            return this;
        }
    }

    public class FormDescriptor {
        public string Name { get; set; }
        public Func<IShapeFactory, dynamic> Shape { get; set; }

        /// <summary>
        /// Contains the logic to be executed when the state is imported, like adapting content identities
        /// </summary>
        public Action<dynamic, ImportContentContext> Import { get; set; }
        /// <summary>
        /// Contains the logic to be executed when the state is imported, like adapting content identities
        /// </summary>
        public Action<dynamic, ExportContentContext> Export { get; set; }
    }
}