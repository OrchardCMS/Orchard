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

            _orchardWebPath = baseDir.Parent.Parent.Parent.Combine("Orchard.Web");

            baseDir.Combine("Hosting").Combine(templateName)
                .DeepCopy(_tempSite);

            _orchardWebPath.Combine("bin")
                .ShallowCopy("*.dll", _tempSite.Combine("bin"))
                .ShallowCopy("*.pdb", _tempSite.Combine("bin"));

            baseDir
                .ShallowCopy("*.dll", _tempSite.Combine("bin"))
                .ShallowCopy("*.pdb", _tempSite.Combine("bin"));

            PhysicalDirectory = _tempSite;
            VirtualDirectory = virtualDirectory;

            _webHostAgent = (WebHostAgent)ApplicationHost.CreateApplicationHost(typeof(WebHostAgent), VirtualDirectory, PhysicalDirectory);

        }

        public void CopyExtension(string extensionFolder, string extensionName) {
            var sourceModule = _orchardWebPath.Combine(extensionFolder).Combine(extensionName);
            var targetModule = _tempSite.Combine(extensionFolder).Combine(extensionName);
            sourceModule.ShallowCopy("*.txt", targetModule);
            if (sourceModule.Combine("Views").IsDirectory)
                sourceModule.Combine("Views").DeepCopy(targetModule.Combine("Views"));
        }

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