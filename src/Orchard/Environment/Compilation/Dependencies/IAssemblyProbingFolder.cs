using System;
using System.Reflection;

namespace Orchard.Environment.Compilation.Dependencies {
    /// <summary>
    /// Abstraction over the folder configued in web.config as an additional 
    /// location to load assemblies from. This assumes a local physical file system,
    /// since Orchard will need to store assembly files locally.
    /// </summary>
    public interface IAssemblyProbingFolder : ISingletonDependency {
        /// <summary>
        /// Return "true" if the assembly corresponding to "moduleName" is
        /// present in the folder.
        /// </summary>
        /// <param name="moduleName">The module's name.</param>
        /// <returns>True if the module exists; false otherwise.</returns>
        bool AssemblyExists(string moduleName);

        /// <summary>
        /// Return the last modification date of the assembly corresponding
        /// to "moduleName". The assembly must be exist on disk, otherwise an
        /// exception is thrown.
        /// </summary>
        /// <param name="moduleName">The module's name.</param>
        /// <returns>The last modification date of the assembly.</returns>
        DateTime GetAssemblyDateTimeUtc(string moduleName);

        /// <summary>
        /// Return the virtual path of the assembly (optional)
        /// </summary>
        /// <param name="moduleName">The module's name.</param>
        /// <returns>The virtual path of the assembly.</returns>
        string GetAssemblyVirtualPath(string moduleName);

        /// <summary>
        /// Load the assembly corresponding to "moduleName" if the assembly file
        /// is present in the folder.
        /// </summary>
        /// <param name="moduleName">The module's name.</param>
        /// <returns>Reference to the loaded assembly.</returns>
        Assembly LoadAssembly(string moduleName);

        /// <summary>
        /// Ensure the assembly corresponding to "moduleName" is removed from the folder
        /// </summary>
        /// <param name="moduleName">The module's name.</param>
        void DeleteAssembly(string moduleName);

        /// <summary>
        /// Store an assembly corresponding to "moduleName" from the given fileName
        /// </summary>
        /// <param name="moduleName">The module's name.</param>
        /// <param name="fileName">The file name for the stored assembly.</param>
        void StoreAssembly(string moduleName, string fileName);
    }
}
