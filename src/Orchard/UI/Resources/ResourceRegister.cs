using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using JetBrains.Annotations;
using Microsoft.WebPages;

namespace Orchard.UI.Resources {
    public class ResourceRegister {
        private string _baseVirtualPath;

        public ResourceRegister(IViewDataContainer container, IResourceManager resourceManager, string resourceType) {
            var templateControl = container as TemplateControl;
            if (templateControl != null) {
                _baseVirtualPath = templateControl.AppRelativeVirtualPath;
            }
            else {
                var webPage = container as WebPageBase;
                if (webPage != null) {
                    _baseVirtualPath = webPage.VirtualPath;
                }
            }
            ResourceManager = resourceManager;
            ResourceType = resourceType;
        }

        protected IResourceManager ResourceManager { get; private set; }
        protected string ResourceType { get; private set; }

        public RequireSettings Require(string resourceName) {
            return Require(resourceName, (string)null);
        }

        public virtual RequireSettings Require(string resourceName, string minimumVersion) {
            var settings = ResourceManager.Require(ResourceType, resourceName)
                .WithMinimumVersion(minimumVersion);
            if (_baseVirtualPath != null) {
                settings.WithBasePath(ResourceDefinition.GetBasePathFromViewPath(ResourceType, _baseVirtualPath));
            }
            return settings;
        }
    }

    public abstract class ScriptRegister : ResourceRegister {
        protected ScriptRegister(IViewDataContainer container, IResourceManager resourceManager) : base(container, resourceManager, "script") {
        }

        public abstract IDisposable Head();
        public abstract IDisposable Foot();
    }
}
