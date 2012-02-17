using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Autofac;
using Autofac.Core;
using Module = Autofac.Module;

namespace Orchard.Environment {
    /// <summary>
    /// Alter components instantiations by setting property values defined in a configuration file
    /// </summary>
    public class HostComponentsConfigModule : Module {
        public static class XNames {
            public const string Xmlns = "";
            public static readonly XName HostComponents = XName.Get("HostComponents", Xmlns);
            public static readonly XName Components = XName.Get("Components", Xmlns);
            public static readonly XName Component = XName.Get("Component", Xmlns);
            public static readonly XName Properties = XName.Get("Properties", Xmlns);
            public static readonly XName Property = XName.Get("Property", Xmlns);
            public static readonly XName Type = XName.Get("Type");
            public static readonly XName Name = XName.Get("Name");
            public static readonly XName Value = XName.Get("Value");
        }

        // component type name => list of [property name, property value]
        public class PropertyEntry {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public readonly IDictionary<string, IEnumerable<PropertyEntry>> _config = new Dictionary<string, IEnumerable<PropertyEntry>>();

        public HostComponentsConfigModule() {
            // Called by the framework, as this class is a "Module"
        }

        public HostComponentsConfigModule(string fileName) {
            var doc = XDocument.Load(fileName);
            foreach (var component in doc.Elements(XNames.HostComponents).Elements(XNames.Components).Elements(XNames.Component)) {
                var componentType = Attr(component, XNames.Type);
                if (componentType == null)
                    continue;

                var properties = component
                    .Elements(XNames.Properties)
                    .Elements(XNames.Property)
                    .Select(property => new PropertyEntry { Name = Attr(property, XNames.Name), Value = Attr(property, XNames.Value) })
                    .Where(t => !string.IsNullOrEmpty(t.Name) && !string.IsNullOrEmpty(t.Value))
                    .ToList();

                if (!properties.Any())
                    continue;

                _config.Add(componentType, properties);
            }
        }

        private string Attr(XElement component, XName name) {
            var attr = component.Attribute(name);
            if (attr == null)
                return null;
            return attr.Value;
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {
            var implementationType = registration.Activator.LimitType;

            IEnumerable<PropertyEntry> properties;
            if (!_config.TryGetValue(implementationType.FullName, out properties))
                return;

            // build an array of actions on this type to assign loggers to member properties
            var injectors = BuildPropertiesInjectors(implementationType, properties).ToArray();

            // if there are no logger properties, there's no reason to hook the activated event
            if (!injectors.Any())
                return;

            // otherwise, whan an instance of this component is activated, inject the loggers on the instance
            registration.Activated += (s, e) => {
                foreach (var injector in injectors)
                    injector(e.Context, e.Instance);
            };
        }

        private IEnumerable<Action<IComponentContext, object>> BuildPropertiesInjectors(Type componentType, IEnumerable<PropertyEntry> properties) {
            // Look for settable properties with name in "properties"
            var settableProperties = componentType
                .GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new {
                    PropertyInfo = p,
                    IndexParameters = p.GetIndexParameters(),
                    Accessors = p.GetAccessors(false),
                    PropertyEntry = properties.Where(t => t.Name == p.Name).FirstOrDefault()
                })
                .Where(x => x.PropertyEntry != null) // Must be present in "properties"
                .Where(x => x.IndexParameters.Count() == 0) // must not be an indexer
                .Where(x => x.Accessors.Length != 1 || x.Accessors[0].ReturnType == typeof(void)); //must have get/set, or only set

            // Return an array of actions that assign the property value
            foreach (var entry in settableProperties) {
                var propertyInfo = entry.PropertyInfo;
                var propertyEntry = entry.PropertyEntry;

                yield return (ctx, instance) => {
                                 object value;
                                 if (ChangeToCompatibleType(propertyEntry.Value, propertyInfo.PropertyType, out value))
                                     propertyInfo.SetValue(instance, value, null);
                             };
            }
        }

        public static bool ChangeToCompatibleType(string value, Type destinationType, out object result) {
            if (string.IsNullOrEmpty(value)) {
                result = null;
                return false;
            }

            if (destinationType.IsInstanceOfType(value)) {
                result = value;
                return true;
            }

            try {
                result = TypeDescriptor.GetConverter(destinationType).ConvertFrom(value);
                return true;
            }
            catch {
                result = null;
                return false;
            }
        }
    }
}