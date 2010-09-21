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
        private string _viewVirtualPath;

        public ResourceRegister(IViewDataContainer container, IResourceManager resourceManager, string resourceType) {
            var templateControl = container as TemplateControl;
            if (templateControl != null) {
                _viewVirtualPath = templateControl.AppRelativeVirtualPath;
            }
            else {
                var webPage = container as WebPageBase;
                if (webPage != null) {
                    _viewVirtualPath = webPage.VirtualPath;
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

        public RequireSettings Include(string resourcePath) {
            if (resourcePath == null) {
                throw new ArgumentNullException("resourcePath");
            }
            return ResourceManager.Include(ResourceType, resourcePath, ResourceDefinition.GetBasePathFromViewPath(ResourceType, _viewVirtualPath));
        }

        public virtual RequireSettings Require(string resourceName, string minimumVersion) {
            if (resourceName == null) {
                throw new ArgumentNullException("resourceName");
            }
            var settings = ResourceManager.Require(ResourceType, resourceName)
                .WithMinimumVersion(minimumVersion);
            if (_viewVirtualPath != null) {
                settings.WithBasePath(ResourceDefinition.GetBasePathFromViewPath(ResourceType, _viewVirtualPath));
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
