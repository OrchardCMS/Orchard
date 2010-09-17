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
        private readonly string _resourceType;
        private readonly IResourceManager _resourceManager;

        public ResourceRegister(IViewDataContainer container, IResourceManager resourceManager, string resourceType) {
            _templateContainer = container as TemplateControl;
            _resourceManager = resourceManager;
            _resourceType = resourceType;
        }

        public RequireSettings Require(string resourceName) {
            return Require(resourceName, (string)null);
        }

        public virtual RequireSettings Require(string resourceName, string minimumVersion) {
            var settings = _resourceManager.Require(_resourceType, resourceName)
                .WithMinimumVersion(minimumVersion);
            if (_templateContainer != null) {
                settings.WithBasePath(ResourceDefinition.GetBasePathFromViewPath(_resourceType, _templateContainer.AppRelativeVirtualPath));
            }
            return settings;
        }
    }

    public class ScriptRegister : ResourceRegister {
        public ScriptRegister(IViewDataContainer container, IResourceManager resourceManager) : base(container, resourceManager, "script") {
        }
        // todo: Head/Tail registration
    }
}
