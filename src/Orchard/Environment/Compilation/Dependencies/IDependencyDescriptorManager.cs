using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Compilation.Dependencies {
    public class DependencyDescriptor {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyDescriptor"/> class. 
        /// </summary>
        public DependencyDescriptor() {
            References = Enumerable.Empty<DependencyReferenceDescriptor>();
        }

        /// <summary>
        /// Gets or sets the name of the dependency.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the loader name.
        /// </summary>
        public string LoaderName { get; set; }

        /// <summary>
        /// Gets or sets the virtual path to the dependency.
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// Gets or sets the dependency's collection of references.
        /// </summary>
        public IEnumerable<DependencyReferenceDescriptor> References { get; set; }
    }

    public class DependencyReferenceDescriptor {
        public string Name { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
    }

    public interface IDependencyDescriptorManager : ISingletonDependency {
        DependencyDescriptor GetDescriptor(string moduleName);
        IEnumerable<DependencyDescriptor> LoadDescriptors();
        void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors);
    }
}
