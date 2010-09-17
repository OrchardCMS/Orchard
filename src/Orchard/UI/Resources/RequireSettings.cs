using System;

namespace Orchard.UI.Resources {
    public class RequireSettings {
        public string BasePath { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Culture { get; set; }
        public bool DebugMode { get; set; }
        public bool CdnMode { get; set; }
        public string MinimumVersion { get; set; }
        public ResourceLocation Location { get; set; }
        public Action<ResourceDefinition> InlineDefinition { get; set; }

        public RequireSettings Combine(RequireSettings other) {
            return new RequireSettings {
                Name = Name,
                Type = Type,
                InlineDefinition = other.InlineDefinition ?? InlineDefinition,
                BasePath = String.IsNullOrEmpty(other.BasePath) ? BasePath : other.BasePath,
                CdnMode = CdnMode || other.CdnMode,
                DebugMode = DebugMode || other.DebugMode,
                Culture = String.IsNullOrEmpty(other.Culture) ? Culture : other.Culture,
                MinimumVersion = String.IsNullOrEmpty(MinimumVersion)
                    ? other.MinimumVersion
                    : MinimumVersion.CompareTo(other.MinimumVersion) > 0 ? other.MinimumVersion : MinimumVersion,
                // if head is specified it takes precedence since it's safer than foot
                Location = (ResourceLocation) Math.Max((int)Location, (int)other.Location),
            };
        }
    }
}
