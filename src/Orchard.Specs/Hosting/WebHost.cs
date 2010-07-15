using System;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using Orchard.Specs.Util;
using Path = Bleroy.FluentPath.Path;

namespace Orchard.Specs.Hosting {
    public class WebHost {
        private WebHostAgent _webHostAgent;
        private Path _tempSite;
        private Path _orchardWebPath;


        public void Initialize(string templateName, string virtualDirectory) {
            var baseDir = Path.Get(AppDomain.CurrentDomain.BaseDirectory);

            _tempSite = Path.Get(System.IO.Path.GetTempFileName()).Delete().CreateDirectory();

            // Trying the two known relative paths to the Orchard.Web directory.
            // The second one is for the target "spec" in orchard.proj.
            _orchardWebPath = baseDir.Up(3).Combine("Orchard.Web");
            if (!_orchardWebPath.Exists) {
                _orchardWebPath = baseDir.Parent.Combine("stage");
            }

            baseDir.Combine("Hosting").Combine(templateName)
                .DeepCopy(_tempSite);

            _orchardWebPath.Combine("bin")
                .ShallowCopy("*.dll", _tempSite.Combine("bin"))
                .ShallowCopy("*.pdb", _tempSite.Combine("bin"));

            // Copy SqlCe binaries
            if (_orchardWebPath.Combine("bin").Combine("x86").IsDirectory) {
                _orchardWebPath.Combine("bin").Combine("x86")
                    .ShallowCopy("*.dll", _tempSite.Combine("bin").Combine("x86"))
                    .ShallowCopy("*.pdb", _tempSite.Combine("bin").Combine("x86"));
            }

            if (_orchardWebPath.Combine("bin").Combine("amd64").IsDirectory) {
                _orchardWebPath.Combine("bin").Combine("amd64")
                    .ShallowCopy("*.dll", _tempSite.Combine("bin").Combine("amd64"))
                    .ShallowCopy("*.pdb", _tempSite.Combine("bin").Combine("amd64"));
            }

            baseDir
                .ShallowCopy("*.dll", _tempSite.Combine("bin"))
                .ShallowCopy("*.exe", _tempSite.Combine("bin"))
                .ShallowCopy("*.pdb", _tempSite.Combine("bin"));

            HostName = "localhost";
            PhysicalDirectory = _tempSite;
            VirtualDirectory = virtualDirectory;

            _webHostAgent = (WebHostAgent)ApplicationHost.CreateApplicationHost(typeof(WebHostAgent), VirtualDirectory, PhysicalDirectory);

        }

        public void Dispose() {
            if (_webHostAgent != null) {
                _webHostAgent.Shutdown();
                _webHostAgent = null;
            }
        }

        public void CopyExtension(string extensionFolder, string extensionName) {
            var sourceModule = _orchardWebPath.Combine(extensionFolder).Combine(extensionName);
            var targetModule = _tempSite.Combine(extensionFolder).Combine(extensionName);

            sourceModule.ShallowCopy("*.txt", targetModule);

            //sourceModule.ShallowCopy("*.csproj", targetModule);
            //sourceModule.DeepCopy("*.cs", targetModule);

            if (sourceModule.Combine("bin").IsDirectory) {
                sourceModule.Combine("bin").ShallowCopy("*.dll", targetModule.Combine("bin"));
                sourceModule.Combine("bin").ShallowCopy("*.exe", targetModule.Combine("bin"));
                sourceModule.Combine("bin").ShallowCopy("*.pdb", targetModule.Combine("bin"));
            }

            if (sourceModule.Combine("Views").IsDirectory)
                sourceModule.Combine("Views").DeepCopy(targetModule.Combine("Views"));
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