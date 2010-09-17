using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;

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

        public void Require(string resourceName) {
            Require(resourceName, null, null);
        }

        public void Require(string resourceName, string minimumVersion) {
            Require(resourceName, minimumVersion, null);
        }

        public void Require(string resourceName, Action<ResourceDefinition> inlineDefinition) {
            Require(resourceName, null, inlineDefinition);
        }

        public void Require(string resourceName, string minimumVersion, Action<ResourceDefinition> inlineDefinition) {
            Require(resourceName, new RequireSettings {
                MinimumVersion = minimumVersion,
                InlineDefinition = inlineDefinition,
            });
        }

        public void Require(RequireSettings settings) {
            Require(settings.Name, settings);
        }

        protected void Require(string resourceName, RequireSettings settings) {
            if (settings == null) {
                throw new ArgumentNullException("settings");
            }
            settings.Type = String.IsNullOrEmpty(settings.Type) ? _resourceType : settings.Type;
            settings.Name = String.IsNullOrEmpty(settings.Name) ? resourceName : settings.Name;
            if (_templateContainer != null) {
                settings.BasePath = ResourceDefinition.GetBasePathFromViewPath(_resourceType, _templateContainer.AppRelativeVirtualPath);
            }
            _resourceManager.Require(settings);
        }

    }

    public class ScriptRegister : ResourceRegister {
        public ScriptRegister(IViewDataContainer container, IResourceManager resourceManager) : base(container, resourceManager, "script") {
        }

        public void RequireFoot(string scriptName) {
            RequireFoot(scriptName, null, null);
        }

        public void RequireFoot(string scriptName, string minimumVersion) {
            RequireFoot(scriptName, minimumVersion, null);
        }

        public void RequireFoot(string scriptName, Action<ResourceDefinition> inlineDefinition) {
            RequireFoot(scriptName, null, inlineDefinition);
        }

        public void RequireFoot(string scriptName, string minimumVersion, Action<ResourceDefinition> inlineDefinition) {
            Require(scriptName, new RequireSettings {
                MinimumVersion = minimumVersion,
                InlineDefinition = inlineDefinition,
                Location = ResourceLocation.Foot
            });
        }

        public void RequireFoot(RequireSettings settings) {
            settings.Location = ResourceLocation.Foot;
            Require(settings.Name, settings);
        }
    }
}
