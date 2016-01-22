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

        /// <summary>
        /// Includes a resource with the specified path
        /// </summary>
        /// <param name="resourcePath">The relative or absolute path of the resource</param>
        public RequireSettings Include(string resourcePath) {
            if (resourcePath == null) {
                throw new ArgumentNullException("resourcePath");
            }
            return ResourceManager.Include(ResourceType, resourcePath, null, ResourceDefinition.GetBasePathFromViewPath(ResourceType, _viewVirtualPath));
        }

        /// <summary>
        /// Includes a resource with the specified path
        /// </summary>
        /// <param name="resourceDebugPath">The relative or absolute path of the resource to be used in debug mode</param>
        /// <param name="resourcePath">The relative or absolute path of the resource</param>
        public RequireSettings Include(string resourceDebugPath, string resourcePath) {
            if (resourcePath == null) {
                throw new ArgumentNullException("resourcePath");
            }
            return ResourceManager.Include(ResourceType, resourcePath, resourceDebugPath, ResourceDefinition.GetBasePathFromViewPath(ResourceType, _viewVirtualPath));
        }

        /// <summary>
        /// Includes a resource that is already defined in a resource manifest
        /// </summary>
        /// <remarks>
        /// You can define resources in resource manifest files with ResourceManifestBuilder.
        /// For examples take a look at any ResourceManifest.cs file.
        /// </remarks>
        /// <param name="resourceName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Uses scripts that were registered to appear in the head of the page
        /// </summary>
        public abstract IDisposable Head();

        /// <summary>
        /// Uses scripts that were registered to appear at the foot of the page
        /// </summary>
        /// <returns></returns>
        public abstract IDisposable Foot();
    }
}
