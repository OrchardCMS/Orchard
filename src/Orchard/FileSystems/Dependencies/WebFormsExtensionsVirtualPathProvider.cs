using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.FileSystems.Dependencies {
    /// <summary>
    /// The purpose of this class is to insert assembly directives in WebForms views served
    /// from ~/Modules and ~/Themes. Inserting these directives ensures that the WebForms
    /// build provider will add proper assembly references when calling the compiler.
    /// For example, if Module A depends on Module Bar and some other 3rd party DLL "Foo.dll",
    /// we will insert the following assembly directives to the view file when server:
    /// &lt;%@ Assembly Src="~/Modules/Bar/Bar.csproj"%&gt;
    /// &lt;%@ Assembly Name="Foo"%&gt;
    /// </summary>
    public class WebFormVirtualPathProvider : VirtualPathProvider, ICustomVirtualPathProvider {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IExtensionDependenciesManager _extensionDependenciesManager;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly string[] _modulesPrefixes = { "~/Modules/" };
        private readonly string[] _themesPrefixes = { "~/Themes/" };
        private readonly string[] _extensions = { ".ascx", ".aspx", ".master" };

        public WebFormVirtualPathProvider(IDependenciesFolder dependenciesFolder, IExtensionDependenciesManager extensionDependenciesManager, IEnumerable<IExtensionLoader> loaders) {
            _dependenciesFolder = dependenciesFolder;
            _extensionDependenciesManager = extensionDependenciesManager;
            _loaders = loaders;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override bool DirectoryExists(string virtualDir) {
            return Previous.DirectoryExists(virtualDir);
        }

        public override bool FileExists(string virtualPath) {
            return Previous.FileExists(virtualPath);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies) {
            var result = GetFileHashWorker(virtualPath, virtualPathDependencies);
            Logger.Debug("GetFileHash(\"{0}\"): {1}", virtualPath, result);
            return result;
        }

        private string GetFileHashWorker(string virtualPath, IEnumerable virtualPathDependencies) {
            // We override the "GetFileHash()" behavior to take into account the dependencies folder
            // state. This ensures that if any dependency changes, ASP.NET will recompile views that
            // have been customized to include custom assembly directives.
            var file = GetModuleVirtualOverride(virtualPath) ?? GetThemeVirtualOverride(virtualPath);
            if (file == null) {
                return base.GetFileHash(virtualPath, virtualPathDependencies);
            }

            var dependencies =
                virtualPathDependencies
                    .OfType<string>()
                    .Concat(file.Loaders.SelectMany(dl => _extensionDependenciesManager.GetVirtualPathDependencies(dl.Descriptor.Name)))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("GetFileHash(\"{0}\") - virtual path dependencies:", virtualPath);
                foreach (var dependency in dependencies) {
                    Logger.Debug("  Dependency: \"{0}\"", dependency);
                }
            }
            return base.GetFileHash(virtualPath, dependencies);
        }

        public override VirtualFile GetFile(string virtualPath) {
            //Logger.Debug("GetFile(\"{0}\")", virtualPath);
            var actualFile = Previous.GetFile(virtualPath);

            return GetModuleCustomVirtualFile(virtualPath, actualFile) ??
                   GetThemeCustomVirtualFile(virtualPath, actualFile) ??
                   actualFile;
        }

        private VirtualFile GetModuleCustomVirtualFile(string virtualPath, VirtualFile actualFile) {
            var file = GetModuleVirtualOverride(virtualPath);
            if (file == null)
                return null;

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("Virtual file from module \"{0}\" served with specific assembly directive:", file.ModuleName);
                Logger.Debug("  Virtual path:       {0}", virtualPath);
                Logger.Debug("  Assembly directive: {0}", file.Directive);
            }

            return new WebFormVirtualFile(virtualPath, actualFile, file.Directive);
        }

        private VirtualFile GetThemeCustomVirtualFile(string virtualPath, VirtualFile actualFile) {
            var file = GetThemeVirtualOverride(virtualPath);
            if (file == null)
                return null;

            if (Logger.IsEnabled(LogLevel.Debug)) {
                Logger.Debug("Virtual file from theme served with specific assembly directive:");
                Logger.Debug("  Virtual path:       {0}", virtualPath);
                Logger.Debug("  Assembly directive: {0}", file.Directive);
            }

            return new WebFormVirtualFile(virtualPath, actualFile, file.Directive);
        }

        private VirtualFileOverride GetModuleVirtualOverride(string virtualPath) {
            var appRelativePath = VirtualPathUtility.ToAppRelative(virtualPath);
            var prefix = PrefixMatch(appRelativePath, _modulesPrefixes);
            if (prefix == null)
                return null;

            var extension = ExtensionMatch(appRelativePath, _extensions);
            if (extension == null)
                return null;

            var moduleName = ModuleMatch(appRelativePath, prefix);
            if (moduleName == null)
                return null;

            var dependencyDescriptor = _dependenciesFolder.GetDescriptor(moduleName);
            if (dependencyDescriptor == null)
                return null;

            var loader = _loaders.Where(l => l.Name == dependencyDescriptor.LoaderName).FirstOrDefault();
            if (loader == null)
                return null;

            var references = loader.GetCompilationReferences(dependencyDescriptor).ToList();
            if (!references.Any())
                return null;

            return new VirtualFileOverride {
                ModuleName = moduleName,
                Directive = CreateAssemblyDirectivesString(references),
                Loaders = new[] { new DependencyLoader { Loader = loader, Descriptor = dependencyDescriptor } }
            };
        }

        private VirtualFileOverride GetThemeVirtualOverride(string virtualPath) {
            var appRelativePath = VirtualPathUtility.ToAppRelative(virtualPath);
            var prefix = PrefixMatch(appRelativePath, _themesPrefixes);
            if (prefix == null)
                return null;

            var extension = ExtensionMatch(appRelativePath, _extensions);
            if (extension == null)
                return null;

            var dependencyLoaders = _loaders
                .SelectMany(loader => _dependenciesFolder
                                          .LoadDescriptors()
                                          .Where(d => d.LoaderName == loader.Name),
                            (loader, desr) => new DependencyLoader { Loader = loader, Descriptor = desr })
                .ToList();

            var references = dependencyLoaders
                .SelectMany(dl => dl.Loader.GetCompilationReferences(dl.Descriptor))
                .ToList();

            if (!references.Any())
                return null;

            return new VirtualFileOverride {
                ModuleName = "",
                Directive = CreateAssemblyDirectivesString(references),
                Loaders = dependencyLoaders
            };
        }

        private string CreateAssemblyDirectivesString(IEnumerable<ExtensionCompilationReference> references) {
            var sb = new StringBuilder();
            foreach (var reference in references) {
                if (!string.IsNullOrEmpty(reference.AssemblyName)) {
                    sb.AppendFormat("<%@ Assembly Name=\"{0}\"%>", reference.AssemblyName);
                }
                if (!string.IsNullOrEmpty(reference.BuildProviderTarget)) {
                    sb.AppendFormat("<%@ Assembly Src=\"{0}\"%>", reference.BuildProviderTarget);
                }
            }
            return sb.ToString();
        }

        private static string ModuleMatch(string virtualPath, string prefix) {
            var index = virtualPath.IndexOf('/', prefix.Length, virtualPath.Length - prefix.Length);
            if (index < 0)
                return null;

            var moduleName = virtualPath.Substring(prefix.Length, index - prefix.Length);
            return (string.IsNullOrEmpty(moduleName) ? null : moduleName);
        }

        private static string ExtensionMatch(string virtualPath, params string[] extensions) {
            return extensions
                .FirstOrDefault(e => virtualPath.EndsWith(e, StringComparison.OrdinalIgnoreCase));
        }

        private static string PrefixMatch(string virtualPath, params string[] prefixes) {
            return prefixes
                .FirstOrDefault(p => virtualPath.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        VirtualPathProvider ICustomVirtualPathProvider.Instance {
            get { return this; }
        }

        private class VirtualFileOverride {
            public string ModuleName { get; set; }
            public string Directive { get; set; }
            public IEnumerable<DependencyLoader> Loaders { get; set; }
        }

        private class DependencyLoader {
            public IExtensionLoader Loader { get; set; }
            public DependencyDescriptor Descriptor { get; set; }
        }
    }
}