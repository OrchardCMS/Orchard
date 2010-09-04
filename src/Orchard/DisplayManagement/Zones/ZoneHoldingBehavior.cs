using System;
using System.Linq;
using ClaySharp;
using ClaySharp.Behaviors;
using ClaySharp.Implementation;
using Orchard.UI;

namespace Orchard.DisplayManagement.Zones {
    public class ZoneHoldingBehavior : ClayBehavior {
        private readonly IShapeFactory _shapeFactory;

        public ZoneHoldingBehavior(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public override object GetMember(Func<object> proceed, object self, string name) {
            if (name == "Zones") {
                return ClayActivator.CreateInstance(new IClayBehavior[] {                
                    new InterfaceProxyBehavior(),
                    new ZonesBehavior(_shapeFactory, self) 
                });
            }
            //Page.Zones.Sidebar.Add(x,"below")
            //Page.Sidebar.Add(x,"below")

            var result = proceed();
            if (((dynamic)result) == null) {
                return ClayActivator.CreateInstance(new IClayBehavior[] { 
                    new NilResultBehavior(),
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
            private readonly string _name;

            public ZoneOnDemandBehavior(IShapeFactory shapeFactory, object parent, string name) {
                _shapeFactory = shapeFactory;
                _parent = parent;
                _name = name;
            }

            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                var argsCount = args.Count();
                if (name == "Add" && (argsCount == 1 || argsCount == 2)) {
                    dynamic parent = _parent;

                    dynamic zone = _shapeFactory.Create("Zone", Arguments.Empty());
                    zone.ZoneName = name;
                    parent[name] = zone;

                    if (argsCount == 1)
                        return zone.Add(args.Single());
                    
                    return zone.Add(args.First(), (string)args.Last());
                }
                return proceed();
            }
        }
    }
}