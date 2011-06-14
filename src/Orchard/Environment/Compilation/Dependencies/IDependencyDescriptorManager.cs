using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Compilation.Dependencies {
    /// <summary>
    /// Provides an abstraction used to manage the dependency descriptors in the dependencies folder.
    /// The dependecy descriptors provide information about the different extensions, and loaders used to load them.
    /// By default they are stored in the dependencies.xml file in the dependencies folder.
    /// </summary>
    public interface IDependencyDescriptorManager : ISingletonDependency {
        /// <summary>
        /// Retrieves the dependency descriptor for a module.
        /// </summary>
        /// <param name="moduleName">The module's name.</param>
        /// <returns>The dependency descriptor for the module.</returns>
        DependencyDescriptor GetDescriptor(string moduleName);

        /// <summary>
        /// Loads the dependency descriptors from the dependencies folder.
        /// </summary>
        /// <returns>A collection of the dependency descriptors.</returns>
        IEnumerable<DependencyDescriptor> LoadDescriptors();

        /// <summary>
        /// Stores a collection of dependencies descriptors into the dependencies folder.
        /// </summary>
        /// <param name="dependencyDescriptors">The collection to be stored.</param>
        void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors);
    }

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
}
