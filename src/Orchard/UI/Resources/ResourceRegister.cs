using System;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.WebPages;

namespace Orchard.UI.Resources {
    public class ResourceRegister {
        private readonly string _viewVirtualPath;

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

        public RequireSettings Include(string resourcePath) {
            if (resourcePath == null) {
                throw new ArgumentNullException("resourcePath");
            }
            return ResourceManager.Include(ResourceType, resourcePath, null, ResourceDefinition.GetBasePathFromViewPath(ResourceType, _viewVirtualPath));
        }

        public RequireSettings Include(string resourceDebugPath, string resourcePath) {
            if (resourcePath == null) {
                throw new ArgumentNullException("resourcePath");
            }
            return ResourceManager.Include(ResourceType, resourcePath, resourceDebugPath, ResourceDefinition.GetBasePathFromViewPath(ResourceType, _viewVirtualPath));
        }

        public virtual RequireSettings Require(string resourceName) {
            if (resourceName == null) {
                throw new ArgumentNullException("resourceName");
            }
            var settings = ResourceManager.Require(ResourceType, resourceName);
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
