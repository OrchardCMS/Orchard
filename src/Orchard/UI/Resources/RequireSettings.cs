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

        public RequireSettings AtHead() {
            return AtLocation(ResourceLocation.Head);
        }

        public RequireSettings AtFoot() {
            return AtLocation(ResourceLocation.Foot);
        }

        public RequireSettings AtLocation(ResourceLocation location) {
            // if head is specified it takes precedence since it's safer than foot
            Location = (ResourceLocation)Math.Max((int)Location, (int)location);
            return this;
        }

        public RequireSettings UseCulture(string cultureName) {
            if (!String.IsNullOrEmpty(cultureName)) {
                Culture = cultureName;
            }
            return this;
        }

        public RequireSettings UseDebugMode() {
            return UseDebugMode(true);
        }

        public RequireSettings UseDebugMode(bool debugMode) {
            DebugMode |= debugMode;
            return this;
        }

        public RequireSettings UseCdn() {
            return UseCdn(true);
        }

        public RequireSettings UseCdn(bool cdn) {
            CdnMode |= cdn;
            return this;
        }

        public RequireSettings WithMinimumVersion(string minimumVersion) {
            MinimumVersion = String.IsNullOrEmpty(MinimumVersion)
                ? minimumVersion
                : (MinimumVersion.CompareTo(minimumVersion) > 0 ? minimumVersion : MinimumVersion);
            return this;
        }

        public RequireSettings WithBasePath(string basePath) {
            BasePath = basePath;
            return this;
        }

        public RequireSettings Define(Action<ResourceDefinition> resourceDefinition) {
            InlineDefinition = resourceDefinition ?? InlineDefinition;
            return this;
        }

        public RequireSettings Combine(RequireSettings other) {
            return (new RequireSettings {
                Name = Name,
                Type = Type
            }).AtLocation(Location).AtLocation(other.Location)
                .WithBasePath(BasePath).WithBasePath(other.BasePath)
                .UseCdn(CdnMode).UseCdn(other.CdnMode)
                .UseDebugMode(DebugMode).UseDebugMode(other.DebugMode)
                .UseCulture(Culture).UseCulture(other.Culture)
                .WithMinimumVersion(MinimumVersion).WithMinimumVersion(other.MinimumVersion)
                .Define(InlineDefinition).Define(other.InlineDefinition);
        }
    }
}
