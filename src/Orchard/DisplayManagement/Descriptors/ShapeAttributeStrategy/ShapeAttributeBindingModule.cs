using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy {
    public class ShapeAttributeBindingModule : Module {
        readonly List<ShapeAttributeOccurrence> _occurrences = new List<ShapeAttributeOccurrence>();

        protected override void Load(ContainerBuilder builder) {
            builder.RegisterInstance(_occurrences).As<IEnumerable<ShapeAttributeOccurrence>>();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {

            var occurrences = registration.Activator.LimitType.GetMethods()
                .SelectMany(mi => mi.GetCustomAttributes(typeof(ShapeAttribute), false).OfType<ShapeAttribute>()
                                      .Select(sa => new ShapeAttributeOccurrence(
                                                        sa,
                                                        mi,
                                                        registration,
                                                        () => GetFeature(registration))))
                .ToArray();

            if (occurrences.Any())
                _occurrences.AddRange(occurrences);
        }

        private static Feature GetFeature(IComponentRegistration registration) {
            object value; return registration.Metadata.TryGetValue("Feature", out value) ? value as Feature : null;
        }
    }
}