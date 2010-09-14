using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeAlterationBuilder {
        protected FeatureDescriptor _feature;
        protected string _shapeType;
        protected readonly IList<Action<ShapeDescriptor>> _configurations = new List<Action<ShapeDescriptor>>();

        public ShapeAlterationBuilder Named(string shapeType) {
            _shapeType = shapeType;
            return this;
        }

        public ShapeAlterationBuilder From(FeatureDescriptor feature) {
            _feature = feature;
            return this;
        }

        public ShapeAlterationBuilder Configure(Action<ShapeDescriptor> action) {
            _configurations.Add(action);
            return this;
        }

        public ShapeAlterationBuilder BoundAs(string bindingSource, Func<ShapeDescriptor, Func<DisplayContext, IHtmlString>> binder) {
            // schedule the configuration
            return Configure(descriptor => {

                Func<DisplayContext, IHtmlString> target = null;

                descriptor.BindingSource = bindingSource;

                // announce the binding, which may be reconfigured before it's used
                descriptor.Binding = displayContext => {

                    // when used, first realize the actual target once
                    if (target == null)
                        target = binder(descriptor);

                    // and execute the re
                    return target(displayContext);
                };
            });
        }

        public ShapeAlterationBuilder OnCreating(Action<ShapeCreatingContext> action) {
            return Configure(descriptor => {
                var existing = descriptor.Creating ?? Enumerable.Empty<Action<ShapeCreatingContext>>();
                descriptor.Creating = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder OnCreated(Action<ShapeCreatedContext> action) {
            return Configure(descriptor => {
                var existing = descriptor.Created ?? Enumerable.Empty<Action<ShapeCreatedContext>>();
                descriptor.Created = existing.Concat(new[] { action });
            });
        }
    }
}