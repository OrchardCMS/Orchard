using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Orchard.Specs.Util;
using Path = Bleroy.FluentPath.Path;

namespace Orchard.Specs.Hosting {
    public class WebHost {
        private readonly Path _orchardTemp;
        private WebHostAgent _webHostAgent;
        private Path _tempSite;
        private Path _orchardWebPath;
        private Path _codeGenDir;
        private IEnumerable<string> _knownModules;
        private IEnumerable<string> _knownThemes;
        private IEnumerable<string> _knownBinAssemblies;

        public WebHost(Path orchardTemp) {
            _orchardTemp = orchardTemp;
        }

        public void Initialize(string templateName, string virtualDirectory, DynamicCompilationOption dynamicCompilationOption) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var baseDir = Path.Get(AppDomain.CurrentDomain.BaseDirectory);

            _tempSite = _orchardTemp.Combine(System.IO.Path.GetRandomFileName());
            try { _tempSite.Delete(); }
            catch { }
            
            // Trying the two known relative paths to the Orchard.Web directory.
            // The second one is for the target "spec" in orchard.proj.
            
            _orchardWebPath = baseDir;

            while (!_orchardWebPath.Combine("Orchard.proj").Exists && _orchardWebPath.Parent != null) {
                _orchardWebPath = _orchardWebPath.Parent;
            }

            _orchardWebPath = _orchardWebPath.Combine("src").Combine("Orchard.Web");

            Log("Initialization of ASP.NET host for template web site \"{0}\":", templateName);
            Log(" Source location: \"{0}\"", _orchardWebPath);
            Log(" Temporary location: \"{0}\"", _tempSite);

            _knownModules = _orchardWebPath.Combine("Modules").Directories.Where(d => d.Combine("module.txt").Exists).Select(d => d.FileName).ToList();
            //foreach (var filename in _knownModules)
            //    Log("Available Module: \"{0}\"", filename);

            _knownThemes = _orchardWebPath.Combine("Themes").Directories.Where(d => d.Combine("theme.txt").Exists).Select(d => d.FileName).ToList();
            //foreach (var filename in _knownThemes)
            //    Log("Available Theme: \"{0}\"", filename);

            _knownBinAssemblies = _orchardWebPath.Combine("bin").GetFiles("*.dll").Select(f => f.FileNameWithoutExtension);
            //foreach (var filename in _knownBinAssemblies)
            //    Log("Assembly in ~/bin: \"{0}\"", filename);

            Log("Copy files from template \"{0}\"", templateName);
            baseDir.Combine("Hosting").Combine(templateName)
                .DeepCopy(_tempSite);

            if (dynamicCompilationOption != DynamicCompilationOption.Enabled) {
                var sourceConfig = baseDir.Combine("Hosting").Combine("TemplateConfigs");
                var siteConfig = _tempSite.Combine("Config");
                switch (dynamicCompilationOption) {
                    case DynamicCompilationOption.Disabled:
                        File.Copy(sourceConfig.Combine("DisableDynamicCompilation.HostComponents.config"), siteConfig.Combine("HostComponents.config"));
                        break;
                    case DynamicCompilationOption.Force:
                        File.Copy(sourceConfig.Combine("ForceDynamicCompilation.HostComponents.config"), siteConfig.Combine("HostComponents.config"));
                        break;
                }
            }

            Log("Copy binaries of the \"Orchard.Web\" project");
            _orchardWebPath.Combine("bin")
                .ShallowCopy("*.dll", _tempSite.Combine("bin"))
                .ShallowCopy("*.pdb", _tempSite.Combine("bin"));

            Log("Copy SqlCe native binaries");
            if (_orchardWebPath.Combine("bin").Combine("x86").IsDirectory) {
                _orchardWebPath.Combine("bin").Combine("x86")
                    .DeepCopy("*.*", _tempSite.Combine("bin").Combine("x86"));
            }

            if (_orchardWebPath.Combine("bin").Combine("amd64").IsDirectory) {
                _orchardWebPath.Combine("bin").Combine("amd64")
                    .DeepCopy("*.*", _tempSite.Combine("bin").Combine("amd64"));
            }

            // Copy binaries of this project, so that remote execution of lambda
            // can be achieved through serialization to the ASP.NET appdomain
            // (see Execute(Action) method)
            Log("Copy Orchard.Specflow test project binaries");
            baseDir.ShallowCopy(
                path => IsSpecFlowTestAssembly(path) && !_tempSite.Combine("bin").Combine(path.FileName).Exists, 
                _tempSite.Combine("bin"));

            Log("Copy Orchard recipes");
            _orchardWebPath.Combine("Modules").Combine("Orchard.Setup").Combine("Recipes").DeepCopy("*.xml", _tempSite.Combine("Modules").Combine("Orchard.Setup").Combine("Recipes"));

            StartAspNetHost(virtualDirectory);

            Log("ASP.NET host initialization completed in {0} sec", stopwatch.Elapsed.TotalSeconds);
        }

        private void StartAspNetHost(string virtualDirectory) {
            Log("Starting up ASP.NET host");
            HostName = "localhost";
            PhysicalDirectory = _tempSite;
            VirtualDirectory = virtualDirectory;

            _webHostAgent = (WebHostAgent)ApplicationHost.CreateApplicationHost(typeof(WebHostAgent), VirtualDirectory, PhysicalDirectory);


            var shuttle = new Shuttle();
            Execute(() => { shuttle.CodeGenDir = HttpRuntime.CodegenDir; });

            // ASP.NET folder seems to be always nested into an empty directory
            _codeGenDir = shuttle.CodeGenDir;
            _codeGenDir = _codeGenDir.Parent;
            Log("ASP.NET CodeGenDir: \"{0}\"", _codeGenDir);
        }

        [Serializable]
        public class Shuttle {
            public string CodeGenDir;
        }

        public void Dispose() {
            if (_webHostAgent != null) {
                _webHostAgent.Shutdown();
                _webHostAgent = null;
            }
            Clean();
        }

        private void Log(string format, params object[] args) {
            Trace.WriteLine(string.Format(format, args));
        }

        public void Clean() {
            // Try to delete temporary files for up to ~1.2 seconds.
            for (int i = 0; i < 4; i++) {
                Log("Waiting 300msec before trying to delete temporary files");
                Thread.Sleep(300);

                if (TryDeleteTempFiles(i == 4)) {
                    Log("Successfully deleted all temporary files");
                    break;
                }
            }
        }

        private bool TryDeleteTempFiles(bool lastTry) {
            var result = true;
            if (_codeGenDir != null && _codeGenDir.Exists) {
                Log("Trying to delete temporary files at \"{0}\"", _codeGenDir);
                try {
                    _codeGenDir.Delete(true); // <- clean as much as possible
                }
                catch (Exception e) {
                    if (lastTry)
                        Log("Failure: \"{0}\"", e);
                    result = false;
                }
            }

            if (_tempSite != null && _tempSite.Exists)
                try {
                    Log("Trying to delete temporary files at \"{0}\"", _tempSite);
                    _tempSite.Delete(true); // <- progressively clean as much as possible
                }
                catch (Exception e) {
                    if (lastTry)
                        Log("failure: \"{0}\"", e);
                    result = false;
                }

            return result;
        }

        public void CopyExtension(string extensionFolder, string extensionName, ExtensionDeploymentOptions deploymentOptions) {
            Log("Copy extension \"{0}\\{1}\" (options={2})", extensionFolder, extensionName, deploymentOptions);
            var sourceModule = _orchardWebPath.Combine(extensionFolder).Combine(extensionName);
            var targetModule = _tempSite.Combine(extensionFolder).Combine(extensionName);

            sourceModule.ShallowCopy("*.txt", targetModule);
            sourceModule.ShallowCopy("*.info", targetModule);
            sourceModule.ShallowCopy("*.config", targetModule);

            if ((deploymentOptions & ExtensionDeploymentOptions.SourceCode) == ExtensionDeploymentOptions.SourceCode) {
                sourceModule.ShallowCopy("*.csproj", targetModule);
                sourceModule.DeepCopy("*.cs", targetModule);
            }

            if (sourceModule.Combine("bin").IsDirectory) {
                sourceModule.Combine("bin").ShallowCopy(path => IsExtensionBinaryFile(path, extensionName, deploymentOptions), targetModule.Combine("bin"));
            }

            if (sourceModule.Combine("Views").IsDirectory)
                sourceModule.Combine("Views").DeepCopy(targetModule.Combine("Views"));

            // don't copy content folders as they are useless in this headless scenario
        }

        public void CopyFile(string source, string destination) {

            StackTrace st = new StackTrace(true);
            Path origin = null;
            foreach(var sf in st.GetFrames()) {
                var sourceFile = sf.GetFileName();
                if(String.IsNullOrEmpty(sourceFile)) {
                    continue;
                }

                var testOrigin = Path.Get(sourceFile).Parent.Combine(source);
                if(testOrigin.Exists) {
                    origin = testOrigin;
                    break;
                }
            }
            
            if(origin == null) {
                throw new FileNotFoundException("File not found: " + source);
            }
            
            var target = _tempSite.Combine(destination);

            Directory.CreateDirectory(target.DirectoryName);
            File.Copy(origin, target);
        }

        private bool IsExtensionBinaryFile(Path path, string extensionName, ExtensionDeploymentOptions deploymentOptions) {
            bool isValidExtension = IsAssemblyFile(path);
            if (!isValidExtension)
                return false;

            bool isAssemblyInWebAppBin = _knownBinAssemblies.Contains(path.FileNameWithoutExtension, StringComparer.OrdinalIgnoreCase);
            if (isAssemblyInWebAppBin)
                return false;

            bool isExtensionAssembly = IsOrchardExtensionFile(path);
            bool copyExtensionAssembly = (deploymentOptions & ExtensionDeploymentOptions.CompiledAssembly) == ExtensionDeploymentOptions.CompiledAssembly;
            if (isExtensionAssembly && !copyExtensionAssembly)
                return false;

            return true;
        }

        private bool IsSpecFlowTestAssembly(Path path) {
            if (!IsAssemblyFile(path))
                return false;

            if (IsOrchardExtensionFile(path))
                return false;

            return true;
        }

        private bool IsAssemblyFile(Path path) {
            return StringComparer.OrdinalIgnoreCase.Equals(path.Extension, ".exe") ||
                   StringComparer.OrdinalIgnoreCase.Equals(path.Extension, ".dll") ||
                   StringComparer.OrdinalIgnoreCase.Equals(path.Extension, ".pdb");
        }

        private bool IsOrchardExtensionFile(Path path) {
            return _knownModules.Where(name => StringComparer.OrdinalIgnoreCase.Equals(name, path.FileNameWithoutExtension)).Any() ||
                   _knownThemes.Where(name => StringComparer.OrdinalIgnoreCase.Equals(name, path.FileNameWithoutExtension)).Any();
        }

        public string HostName { get; set; }
        public string PhysicalDirectory { get; private set; }
        public string VirtualDirectory { get; private set; }

        public string Cookies { get; set; }


        public void Execute(Action action) {
            var shuttleSend = new SerializableDelegate<Action>(action);
            var shuttleRecv = _webHostAgent.Execute(shuttleSend);
            CopyFields(shuttleRecv.Delegate.Target, shuttleSend.Delegate.Target);
        }

        private static void CopyFields<T>(T from, T to) where T : class {
            if (from == null || to == null)
                return;
            foreach (FieldInfo fieldInfo in from.GetType().GetFields()) {
                var value = fieldInfo.GetValue(from);
                fieldInfo.SetValue(to, value);
            }
        }
    }
}