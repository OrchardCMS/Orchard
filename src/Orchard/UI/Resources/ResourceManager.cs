using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;

namespace Orchard.UI.Resources {
    public class ResourceManager : IResourceManager {
        private readonly Dictionary<Tuple<String, String>, RequireSettings> _required = new Dictionary<Tuple<String, String>, RequireSettings>();
        private readonly Lazy<DynamicResourceManifest> _dynamicResourceProvider = new Lazy<DynamicResourceManifest>();
        private readonly List<LinkEntry> _links = new List<LinkEntry>();
        private readonly Dictionary<string, MetaEntry> _metas = new Dictionary<string, MetaEntry>();
        private readonly Dictionary<string, IList<ResourceRequiredContext>> _builtResources = new Dictionary<string, IList<ResourceRequiredContext>>();

        public ResourceManager(IEnumerable<IResourceManifest> resourceProviders) {
            ResourceProviders = resourceProviders;
        }

        public IEnumerable<IResourceManifest> ResourceProviders { get; private set; }

        // represents resources that were required during the request but that had no matching resource provider
        public virtual ResourceManifest DynamicResources {
            get {
                return _dynamicResourceProvider.Value;
            }
        }

        public virtual void Require(RequireSettings settings) {
            RequireSettings existingSettings;
            var key = new Tuple<string, string>(settings.Type, settings.Name);
            if (_required.TryGetValue(key, out existingSettings)) {
                settings = settings.Combine(existingSettings);
            }
            _builtResources[settings.Type] = null;
            _required[key] = settings;
        }

        public virtual void NotRequired(string resourceType, string resourceName) {
            var key = new Tuple<string, string>(resourceType, resourceName);
            _builtResources[resourceType] = null;
            _required.Remove(key);
        }

        public virtual ResourceDefinition FindResource(RequireSettings settings) {
            // find the resource with the given type and name
            // that has at least the given version number. If multiple,
            // return the resource with the greatest version number.
            // If not found and an inlineDefinition is given, define the resource on the fly
            // using the action.
            var name = settings.Name;
            var type = settings.Type;
            var minimumVersion = settings.MinimumVersion;
            var resource = (from p in ResourceProviders
                            from r in p.GetResources(type)
                            where r.Key == name && (String.IsNullOrEmpty(minimumVersion) || String.CompareOrdinal(r.Value.Version ?? "", minimumVersion) >= 0)
                            orderby r.Value.Version descending
                            select r.Value).FirstOrDefault();
            if (resource == null && _dynamicResourceProvider.IsValueCreated) {
                resource = (from r in _dynamicResourceProvider.Value.GetResources(type)
                            where r.Key == name && (String.IsNullOrEmpty(minimumVersion) || String.CompareOrdinal(r.Value.Version ?? "", minimumVersion) >= 0)
                            orderby r.Value.Version descending
                            select r.Value).FirstOrDefault();
            }
            if (resource == null && settings.InlineDefinition != null) {
                // defining it on the fly
                resource = DynamicResources.DefineResource(type, name)
                    .SetBasePath(settings.BasePath)
                    .SetVersion(minimumVersion);
                settings.InlineDefinition(resource);
            }
            return resource;
        }

        public virtual IEnumerable<RequireSettings> GetRequiredResources(string type) {
            return from r in _required
                   where r.Key.Item1 == type
                   select r.Value;
        }

        public virtual IList<LinkEntry> GetRegisteredLinks() {
            return _links.AsReadOnly();
        }

        public virtual IList<MetaEntry> GetRegisteredMetas() {
            return _metas.Values.ToList().AsReadOnly();
        }

        public virtual IList<ResourceRequiredContext> BuildRequiredResources(string resourceType) {
            IList<ResourceRequiredContext> requiredResources;
            if (_builtResources.TryGetValue(resourceType, out requiredResources) && requiredResources != null) {
                return requiredResources;
            }
            var allResources = new OrderedDictionary();
            foreach (var settings in GetRequiredResources(resourceType)) {
                var resource = FindResource(settings);
                if (resource == null) {
                    throw String.IsNullOrEmpty(settings.MinimumVersion)
                        ? new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "A '{1}' named '{0}' could not be found.", settings.Name, settings.Type))
                        : new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "A '{2}' named '{0}' with version greater than or equal to '{1}' could not be found.", settings.Name, settings.MinimumVersion, settings.Type));
                }
                ExpandDependencies(resource, settings, allResources);
            }
            requiredResources = (from DictionaryEntry entry in allResources
                                 select new ResourceRequiredContext {Resource = (ResourceDefinition) entry.Key, Settings = (RequireSettings) entry.Value}).ToList();
            _builtResources[resourceType] = requiredResources;
            return requiredResources;
        }

        protected virtual void ExpandDependencies(ResourceDefinition resource, RequireSettings settings, OrderedDictionary allResources) {
            if (resource == null) {
                return;
            }
            if (allResources.Contains(resource)) {
                settings = ((RequireSettings) allResources[resource]).Combine(settings);
            }
            settings.Type = resource.Type;
            settings.Name = resource.Name;
            if (resource.Dependencies != null) {
                var dependencies = from d in resource.Dependencies
                                   select FindResource(new RequireSettings { Type = resource.Type, Name = d });
                foreach (var dependency in dependencies) {
                    if (dependency == null) {
                        continue;
                    }
                    ExpandDependencies(dependency, settings, allResources);
                }
            }
            allResources[resource] = settings;
        }

        public void RegisterLink(LinkEntry link) {
            _links.Add(link);
        }

        public void SetMeta(MetaEntry meta) {
            if (meta == null || String.IsNullOrEmpty(meta.Name)) {
                return;
            }
            _metas[meta.Name] = meta;
        }

        public void AppendMeta(MetaEntry meta, string contentSeparator) {
            if (meta == null || String.IsNullOrEmpty(meta.Name)) {
                return;
            }
            MetaEntry existingMeta;
            if (_metas.TryGetValue(meta.Name, out existingMeta)) {
                meta = MetaEntry.Combine(existingMeta, meta, contentSeparator);
            }
            _metas[meta.Name] = meta;
        }

    }
}
