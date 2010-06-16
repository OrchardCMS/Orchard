using System;
using System.Reflection;
using Orchard.Caching;

namespace Orchard.FileSystems.Dependencies {
    /// <summary>
    /// Abstraction over the folder configued in web.config as an additional 
    /// location to load assemblies from. This assumes a local physical file system,
    /// since Orchard will need to store assembly files locally.
    /// </summary>
    public interface IAssemblyProbingFolder : IVolatileProvider {
        /// <summary>
        /// Return "true" if the assembly corresponding to "moduleName" is
        /// present in the folder.
        /// </summary>
        bool HasAssembly(string moduleName);

        /// <summary>
        /// Return the last modification date of the assembly corresponding
        /// to "moduleName". The assembly must be exist on disk, otherwise an
        /// exception is thrown.
        /// </summary>
        DateTime GetAssemblyDateTimeUtc(string moduleName);

        /// <summary>
        /// Load the assembly corresponding to "moduleName" if the assembly file
        /// is present in the folder.
        /// </summary>
        Assembly LoadAssembly(string moduleName);

        /// <summary>
        /// Return the physical location where to store the assembly
        /// corresponding to "moduleName". This will return a correct path
        /// even if the assembly is not currently stored in that location.
        /// This method can be used to answer the question "Where would the assembly
        /// for module "moduleName" be stored if it exsisted?"
        /// </summary>
        string GetAssemblyPhysicalFileName(string moduleName);
    }
}