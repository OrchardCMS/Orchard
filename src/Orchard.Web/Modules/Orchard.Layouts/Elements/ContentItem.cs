using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class ContentItem : Element {
        public override string Category {
            get { return "Content"; }
        }

        public override bool HasEditor {
            get { return true; }
        }

        public IEnumerable<int> ContentItemIds {
            get { return Deserialize(State.Get("ContentItemIds")); }
            set { State["ContentItemIds"] = Serialize(value); }
        }

        public string DisplayType {
            get { return State.Get("DisplayType"); }
            set { State["DisplayType"] = value; }
        }

        public static string Serialize(IEnumerable<int> values) {
            return values != null ? String.Join(",", values) : "";
        }

        public static IEnumerable<int> Deserialize(string data) {
            if (String.IsNullOrWhiteSpace(data))
                return Enumerable.Empty<int>();

            var query =
                from x in data.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                let id = x.ToInt32()
                where id != null
                select id.Value;

            return query;
        }
    }
}