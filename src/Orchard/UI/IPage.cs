using System;
using System.Collections.Generic;
using System.Web.Mvc;

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

    public interface IZone {
        string ZoneName { get; set; }
        IZone Add(object item);
        IZone Add(object item, string position);
        IZone Add(Action<HtmlHelper> action);
        IZone Add(Action<HtmlHelper> action, string position);
    }

    public class Zone : IZone {
        private readonly IList<object> _items = new List<object>();

        public virtual string ZoneName { get; set; }

        public virtual IZone Add(object item) {
            _items.Add(item);
            return this;
        }

        public virtual IZone Add(object item, string position) {
            _items.Add(item); // not messing with position at the moment
            return this;
        }

        public virtual IZone Add(Action<HtmlHelper> action) {
            //throw new NotImplementedException();
            return this;
        }

        public virtual IZone Add(Action<HtmlHelper> action, string position) {
            //throw new NotImplementedException();
            return this;
        }
    }
}