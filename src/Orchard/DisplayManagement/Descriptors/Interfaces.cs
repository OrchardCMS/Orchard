using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {

    public interface IShapeTableManager : IDependency {
        ShapeTable GetShapeTable(string themeName);
    }

    public interface IShapeTableFactory : IDependency {
        IDictionary<string, ShapeTable> CreateShapeTables();
    }

    public interface IShapeDescriptorBindingStrategy : IDependency {
        void Discover(ShapeTableBuilder builder);
    }

    public class ShapeTable {
        public IDictionary<string, ShapeDescriptor> Descriptors { get; set; }
    }

    public class ShapeDescriptor {
        public string ShapeType { get; set; }
        
        /// <summary>
        /// The BindingSource is informational text about the source of the Binding delegate. Not used except for 
        /// troubleshooting.
        /// </summary>
        public string BindingSource { get; set; }

        public Func<DisplayContext, IHtmlString> Binding { get; set; }
    }


    public class ShapeTableBuilder {
        readonly IList<ShapeDescriptorAlterationBuilderImpl> _descriptorBuilders = new List<ShapeDescriptorAlterationBuilderImpl>();

        public ShapeDescriptorAlterationBuilder Describe {
            get {
                var db = new ShapeDescriptorAlterationBuilderImpl();
                _descriptorBuilders.Add(db);
                return db;
            }
        }

        public IEnumerable<ShapeDescriptorAlteration> Build() {
            return _descriptorBuilders.Select(b => b.Build());
        }

        class ShapeDescriptorAlterationBuilderImpl : ShapeDescriptorAlterationBuilder {
            public ShapeDescriptorAlteration Build() {
                return new ShapeDescriptorAlteration(_shapeType, _feature, _configurations.ToArray());
            }
        }
    }

    public class ShapeDescriptorAlteration {
        private readonly IList<Action<ShapeDescriptor>> _configurations;

        public ShapeDescriptorAlteration(string shapeType, FeatureDescriptor feature, IList<Action<ShapeDescriptor>> configurations) {
            _configurations = configurations;
            ShapeType = shapeType;
            Feature = feature;
        }

        public string ShapeType { get; private set; }
        public FeatureDescriptor Feature { get; private set; }
        public void Alter(ShapeDescriptor descriptor) {
            foreach (var configuration in _configurations) {
                configuration(descriptor);
            }
        }
    }

    public class ShapeDescriptorAlterationBuilder {
        protected FeatureDescriptor _feature;
        protected string _shapeType;
        protected readonly IList<Action<ShapeDescriptor>> _configurations = new List<Action<ShapeDescriptor>>();

        public ShapeDescriptorAlterationBuilder Named(string shapeType) {
            _shapeType = shapeType;
            return this;
        }

        public ShapeDescriptorAlterationBuilder From(FeatureDescriptor feature) {
            _feature = feature;
            return this;
        }

        public ShapeDescriptorAlterationBuilder Configure(Action<ShapeDescriptor> action) {
            _configurations.Add(action);
            return this;
        }

        public ShapeDescriptorAlterationBuilder BoundAs(string bindingSource, Func<ShapeDescriptor, Func<DisplayContext, IHtmlString>> binder) {
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
    }

}
