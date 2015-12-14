using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.UI {
    public interface IPage {
        IZoneCollection Zones { get; }
    }

    public interface IZoneCollection {
        IZone this[string key] { get; }
    }

    class ZoneCollection : IZoneCollection {
        readonly IDictionary<string, IZone> _zones = new Dictionary<string, IZone>();
        public IZone this[string key] {
            get {
                //a nice race condition
                if (!_zones.ContainsKey(key))
                    _zones[key] = new Zone();
                return _zones[key];
            }
        }
    }

    public interface IZone : IEnumerable {
        string ZoneName { get; set; }
        Shape Add(object item, string position);
        IZone Add(Action<HtmlHelper> action, string position);
    }

    public class Zone : Shape, IZone {
        public virtual string ZoneName { get; set; }

        public IZone Add(Action<HtmlHelper> action, string position) {
            // pszmyd: Replaced the NotImplementedException with simply doing nothing
            return this;
        }
    }
}
