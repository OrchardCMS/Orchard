using System;
using System.Linq;
using ClaySharp;
using ClaySharp.Behaviors;
using ClaySharp.Implementation;
using Orchard.DisplayManagement;

namespace Orchard.UI.Zones {
    /// <summary>
    /// Provides the behavior of shapes that have a Zones property.
    /// Examples include Layout and Item
    /// 
    /// * Returns a fake parent object for zones
    /// Foo.Zones 
    /// 
    /// * 
    /// Foo.Zones.Alpha : 
    /// Foo.Zones["Alpha"] 
    /// Foo.Alpha :same
    /// 
    /// </summary>
    public class ZoneHoldingBehavior : ClayBehavior {
        private readonly IShapeFactory _shapeFactory;

        public ZoneHoldingBehavior(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public override object GetMember(Func<object> proceed, object self, string name) {
            if (name == "Zones") {
                // provide a robot for zone manipulation on parent object
                return ClayActivator.CreateInstance(new IClayBehavior[] {                
                    new InterfaceProxyBehavior(),
                    new ZonesBehavior(_shapeFactory, self) 
                });
            }
            
            var result = proceed();
            if (((dynamic)result) == null) {
                
                // substitute nil results with a robot that turns adds a zone on
                // the parent when .Add is invoked
                return ClayActivator.CreateInstance(new IClayBehavior[] { 
                    new InterfaceProxyBehavior(),
                    new NilBehavior(),
                    new ZoneOnDemandBehavior(_shapeFactory, self, name) 
                });
            }
            return result;
        }

        public class ZonesBehavior : ClayBehavior {
            private readonly IShapeFactory _shapeFactory;
            private readonly object _parent;

            public ZonesBehavior(IShapeFactory shapeFactory, object parent) {
                _shapeFactory = shapeFactory;
                _parent = parent;
            }

            public override object GetMember(Func<object> proceed, object self, string name) {
                var parentMember = ((dynamic)_parent)[name];
                if (parentMember == null) {
                    return ClayActivator.CreateInstance(new IClayBehavior[] { 
                        new InterfaceProxyBehavior(),
                        new NilBehavior(),
                        new ZoneOnDemandBehavior(_shapeFactory, _parent, name) 
                    });
                }
                return parentMember;
            }
            public override object GetIndex(Func<object> proceed, System.Collections.Generic.IEnumerable<object> keys) {
                if (keys.Count() == 1) {
                    return GetMember(proceed, null, System.Convert.ToString(keys.Single()));
                }
                return proceed();
            }
        }

        public class ZoneOnDemandBehavior : ClayBehavior {
            private readonly IShapeFactory _shapeFactory;
            private readonly object _parent;
            private readonly string _potentialZoneName;

            public ZoneOnDemandBehavior(IShapeFactory shapeFactory, object parent, string potentialZoneName) {
                _shapeFactory = shapeFactory;
                _parent = parent;
                _potentialZoneName = potentialZoneName;
            }

            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                var argsCount = args.Count();
                if (name == "Add" && (argsCount == 1 || argsCount == 2)) {
                    dynamic parent = _parent;

                    dynamic zone = _shapeFactory.Create("Zone", Arguments.Empty());
                    zone.Parent(_parent).ZoneName(_potentialZoneName);
                    parent[_potentialZoneName] = zone;

                    if (argsCount == 1)
                        return zone.Add(args.Single());
                    
                    return zone.Add(args.First(), (string)args.Last());
                }
                return proceed();
            }
        }
    }
}