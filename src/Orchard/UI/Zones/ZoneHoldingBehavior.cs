using System;
using System.Linq;
using ClaySharp;
using ClaySharp.Behaviors;

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
        private readonly Func<dynamic> _zoneFactory;
        private readonly dynamic _layoutShape;

        public ZoneHoldingBehavior(Func<dynamic> zoneFactory, dynamic layoutShape) {
            _zoneFactory = zoneFactory;
            _layoutShape = layoutShape;
        }

        public override object GetMember(Func<object> proceed, object self, string name) {
            if (name == "Zones") {
                // provide a robot for zone manipulation on parent object
                return ClayActivator.CreateInstance(new IClayBehavior[] {                
                    new InterfaceProxyBehavior(),
                    new ZonesBehavior(_zoneFactory, self, _layoutShape) 
                });
            }

            var result = proceed();
            if (((dynamic)result) == null) {

                // substitute nil results with a robot that turns adds a zone on
                // the parent when .Add is invoked
                return ClayActivator.CreateInstance(new IClayBehavior[] { 
                    new InterfaceProxyBehavior(),
                    new NilBehavior(),
                    new ZoneOnDemandBehavior(_zoneFactory, self, name) 
                });
            }
            return result;
        }

        public class ZonesBehavior : ClayBehavior {
            private readonly Func<dynamic> _zoneFactory;
            private object _parent;
            private readonly dynamic _layoutShape;

            public ZonesBehavior(Func<dynamic> zoneFactory, object parent, dynamic layoutShape) {
                _zoneFactory = zoneFactory;
                _parent = parent;
                _layoutShape = layoutShape;
            }

            public override object GetMember(Func<object> proceed, object self, string name) {
                var parentMember = ((dynamic)_parent)[name];
                if (parentMember == null) {
                    return ClayActivator.CreateInstance(new IClayBehavior[] { 
                        new InterfaceProxyBehavior(),
                        new NilBehavior(),
                        new ZoneOnDemandBehavior(_zoneFactory, _parent, name) 
                    });
                }
                return parentMember;
            }
            public override object GetIndex(Func<object> proceed, object self, System.Collections.Generic.IEnumerable<object> keys) {
                if (keys.Count() == 1) {
                    var key = System.Convert.ToString(keys.Single());

                    return GetMember(proceed, null, key);
                }
                return proceed();
            }
        }

        public class ZoneOnDemandBehavior : ClayBehavior {
            private readonly Func<dynamic> _zoneFactory;
            private readonly object _parent;
            private readonly string _potentialZoneName;

            public ZoneOnDemandBehavior(Func<dynamic> zoneFactory, object parent, string potentialZoneName) {
                _zoneFactory = zoneFactory;
                _parent = parent;
                _potentialZoneName = potentialZoneName;
            }

            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                var argsCount = args.Count();
                if (name == "Add" && (argsCount == 1 || argsCount == 2)) {
                    // pszmyd: Ignore null shapes
                    if (args.First() == null)
                        return _parent;

                    dynamic parent = _parent;

                    dynamic zone = _zoneFactory();
                    zone.Parent = _parent;
                    zone.ZoneName = _potentialZoneName;
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