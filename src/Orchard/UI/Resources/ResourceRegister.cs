using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using JetBrains.Annotations;

namespace Orchard.UI.Resources {
    public class ResourceRegister {
        private readonly TemplateControl _templateContainer;

        public ResourceRegister(IViewDataContainer container, IResourceManager resourceManager, string resourceType) {
            _templateContainer = container as TemplateControl;
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
            if (_templateContainer != null) {
                settings.WithBasePath(ResourceDefinition.GetBasePathFromViewPath(ResourceType, _templateContainer.AppRelativeVirtualPath));
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
