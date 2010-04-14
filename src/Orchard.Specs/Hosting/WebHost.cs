using System;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using Orchard.Specs.Util;

namespace Orchard.Specs.Hosting
{
    public class WebHost
    {
        private WebHostAgent _webHostAgent;
        private PathUtil _tempSite;
        private PathUtil _orchardWebPath;

        public void Initialize(string templateName, string virtualDirectory)
        {
            _tempSite = PathUtil.GetTempFolder();
            _orchardWebPath = PathUtil.BaseDirectory.Parent.Parent.Parent.Combine("Orchard.Web");
            
            var baseDir = PathUtil.BaseDirectory;
            
            PathUtil.BaseDirectory
                .Combine("Hosting")
                .Combine(templateName)
                .CopyAll(SearchOption.AllDirectories, _tempSite);

            _orchardWebPath.Combine("bin").CopyAll("*.dll", _tempSite.Combine("bin"));
            _orchardWebPath.Combine("bin").CopyAll("*.pdb", _tempSite.Combine("bin"));
            baseDir.CopyAll("*.dll", _tempSite.Combine("bin"));
            baseDir.CopyAll("*.pdb", _tempSite.Combine("bin"));

            PhysicalDirectory = _tempSite.ToString();
            VirtualDirectory = virtualDirectory;

            _webHostAgent = (WebHostAgent)ApplicationHost.CreateApplicationHost(typeof(WebHostAgent), VirtualDirectory, PhysicalDirectory);
        }

        public void CopyExtension(string extensionFolder, string extensionName) {
            var sourceModule = _orchardWebPath.Combine(extensionFolder).Combine(extensionName);
            var targetModule = _tempSite.Combine(extensionFolder).Combine(extensionName);
            sourceModule.CopyAll("*.txt", targetModule);
            if (sourceModule.Combine("Views").DirectoryExists)
                sourceModule.Combine("Views").CopyAll(SearchOption.AllDirectories, targetModule.Combine("Views"));
            
        }

        public string PhysicalDirectory { get; private set; }
        public string VirtualDirectory { get; private set; }

        public void Execute(Action action)
        {
            var shuttleSend = new SerializableDelegate<Action>(action);
            var shuttleRecv = _webHostAgent.Execute(shuttleSend);
            CopyFields(shuttleRecv.Delegate.Target, shuttleSend.Delegate.Target);
        }

        private static void CopyFields<T>(T from, T to) where T : class
        {
            if (from == null || to == null)
                return;
            foreach (FieldInfo fieldInfo in from.GetType().GetFields())
            {
                var value = fieldInfo.GetValue(from);
                fieldInfo.SetValue(to, value);
            }
        }

    }
}