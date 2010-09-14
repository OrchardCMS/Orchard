using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeAlterationBuilder {
        Feature _feature;
        readonly string _shapeType;
        readonly string _bindingName;
        readonly IList<Action<ShapeDescriptor>> _configurations = new List<Action<ShapeDescriptor>>();

        public ShapeAlterationBuilder(Feature feature, string shapeType) {
            _feature = feature;
            _bindingName = shapeType;
            var delimiterIndex = shapeType.IndexOf("__");

            if (delimiterIndex < 0) {
                _shapeType = shapeType;
            }
            else {
                _shapeType = shapeType.Substring(0, delimiterIndex);
            }
        }

        public ShapeAlterationBuilder From(Feature feature) {
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

                var binding = new ShapeBinding {
                    ShapeDescriptor = descriptor,
                    BindingName = _bindingName,
                    BindingSource = bindingSource,
                    Binding = displayContext => {

                        // when used, first realize the actual target once
                        if (target == null)
                            target = binder(descriptor);

                        // and execute the re
                        return target(displayContext);
                    }
                };
                descriptor.Bindings[_shapeType] = binding;

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

        public ShapeAlteration Build() {
            return new ShapeAlteration(_shapeType, _feature, _configurations.ToArray());
        }
    }
}