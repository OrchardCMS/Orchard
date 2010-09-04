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
        void Add(object item);
        void Add(object item, string position);
        void Add(Action<HtmlHelper> action);
        void Add(Action<HtmlHelper> action, string position);
    }

    class Zone : IZone {
        private readonly IList<object> _items = new List<object>();

        public void Add(object item) {
            _items.Add(item);
        }

        public void Add(object item, string position) {
            _items.Add(item); // not messing with position at the moment
        }

        public void Add(Action<HtmlHelper> action) {
            //throw new NotImplementedException();
        }

        public void Add(Action<HtmlHelper> action, string position) {
            //throw new NotImplementedException();
        }
    }
}