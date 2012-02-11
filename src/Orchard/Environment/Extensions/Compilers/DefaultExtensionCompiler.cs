using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Compilers {
    /// <summary>
    /// Compile an extension project file into an assembly
    /// </summary>
    public class DefaultExtensionCompiler : IExtensionCompiler {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IProjectFileParser _projectFileParser;
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly ICriticalErrorProvider _criticalErrorProvider;

        public DefaultExtensionCompiler(
            IVirtualPathProvider virtualPathProvider,
            IProjectFileParser projectFileParser,
            IDependenciesFolder dependenciesFolder,
            IEnumerable<IExtensionLoader> loaders,
            IAssemblyLoader assemblyLoader,
            ICriticalErrorProvider criticalErrorProvider) {

            _virtualPathProvider = virtualPathProvider;
            _projectFileParser = projectFileParser;
            _dependenciesFolder = dependenciesFolder;
            _loaders = loaders;
            _assemblyLoader = assemblyLoader;
            _criticalErrorProvider = criticalErrorProvider;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void Compile(CompileExtensionContext context) {
            Logger.Information("Generate code for file \"{0}\"", context.VirtualPath);
            var moduleName = Path.GetFileNameWithoutExtension(context.VirtualPath);
            var dependencyDescriptor = _dependenciesFolder.GetDescriptor(moduleName);
            if (dependencyDescriptor == null)
                return;

            try {
                var projectFileDescriptor = _projectFileParser.Parse(context.VirtualPath);

                    // Add source files
                    var directory = _virtualPathProvider.GetDirectoryName(context.VirtualPath);
                    foreach (var filename in projectFileDescriptor.SourceFilenames.Select(f => _virtualPathProvider.Combine(directory, f))) {
                        context.AssemblyBuilder.AddCodeCompileUnit(CreateCompileUnit(filename));
                    }

                    var addedReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // Add assembly references
                    foreach (var reference in dependencyDescriptor.References) {
                        var referenceTemp = reference;
                        var loader = _loaders.SingleOrDefault(l => l.Name == referenceTemp.LoaderName);
                        if (loader == null) {
                            Logger.Warning("Could not find loader '{0}' in active loaders", reference.LoaderName);
                            continue;
                        }

                        var assembly = loader.LoadReference(reference);
                        if (assembly == null) {
                            Logger.Warning("Loader '{0}' could not load reference '{1}'", reference.LoaderName, reference.Name);
                            continue;
                        }

                        addedReferences.Add(reference.Name);
                        context.AssemblyBuilder.AddAssemblyReference(assembly);
                    }

                    _criticalErrorProvider.Clear();

                    // Load references specified in project file (only the ones not yet loaded)
                    foreach (var assemblyReference in projectFileDescriptor.References) {
                        if (addedReferences.Contains(assemblyReference.SimpleName))
                            continue;

                        var assembly = _assemblyLoader.Load(assemblyReference.FullName);
                        if (assembly != null) {
                            context.AssemblyBuilder.AddAssemblyReference(assembly);
                        }
                        else {
                            Logger.Error("Assembly reference '{0}' for project '{1}' cannot be loaded", assemblyReference.FullName, context.VirtualPath);
                            _criticalErrorProvider.RegisterErrorMessage(T(
                                "The assembly reference '{0}' could not be loaded for module '{1}'.\r\n\r\n" +
                                "There are generally a few ways to solve this issue:\r\n" +
                                "1. Install any dependent module.\r\n" +
                                "2. Remove the assembly reference from the project file if it's not needed.\r\n" +
                                "3. Ensure the assembly reference is present in the 'bin' directory of the module.\r\n" +
                                "4. Ensure the assembly reference is present in the 'bin' directory of the application.\r\n" +
                                "5. Specify the strong name of the assembly (name, version, culture, publickey) if the assembly is present in the GAC.",
                                assemblyReference.FullName, moduleName));
                            throw new OrchardCoreException(T("Assembly reference '{0}' for project '{1}' cannot be loaded", assemblyReference.FullName, context.VirtualPath));
                        }
                    }
                }
            catch (Exception e) {
                //Note: we need to embed the "e.Message" in the exception text because 
                //      ASP.NET build manager "swallows" inner exceptions from this method.
                throw new OrchardCoreException(T("Error compiling module \"{0}\" from file \"{1}\":\r\n{2}", moduleName, context.VirtualPath, e.Message), e);
            }
        }

        private CodeCompileUnit CreateCompileUnit(string virtualPath) {
            var contents = GetContents(virtualPath);
            var unit = new CodeSnippetCompileUnit(contents);
            var physicalPath = _virtualPathProvider.MapPath(virtualPath);
            if (!string.IsNullOrEmpty(physicalPath)) {
                unit.LinePragma = new CodeLinePragma(physicalPath, 1);
            }
            return unit;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private string GetContents(string virtualPath) {
            string contents;
            using (var stream = _virtualPathProvider.OpenFile(virtualPath)) {
                using (var reader = new StreamReader(stream)) {
                    contents = reader.ReadToEnd();
                }
            }
            return contents;
        }
    }
}