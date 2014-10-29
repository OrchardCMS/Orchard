using System;
using System.Globalization;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class Projection : Element {

        public override bool HasEditor {
            get { return true; }
        }

        public override string Category {
            get { return "Content"; }
        }

        public string QueryLayoutId {
            get { return State.Get("QueryLayoutId"); }
            set { State["QueryLayoutId"] = value; }
        }

        public int? QueryId {
            get {
                return String.IsNullOrWhiteSpace(QueryLayoutId) ? null : QueryLayoutId.Split(new[] { ';' })[0].ToInt32();
            }
        }

        public int? LayoutId {
            get {
                return String.IsNullOrWhiteSpace(QueryLayoutId) ? null : QueryLayoutId.Split(new[] { ';' })[1].ToInt32();
            }
        }

        public int ItemsToDisplay {
            get { return State.Get("ItemsToDisplay").ToInt32().GetValueOrDefault(); }
            set { State["ItemsToDisplay"] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public int Skip {
            get { return State.Get("Skip").ToInt32().GetValueOrDefault(); }
            set { State["Skip"] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public int MaxItems {
            get { return State.Get("MaxItems").ToInt32().GetValueOrDefault(); }
            set { State["MaxItems"] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public string PagerSuffix {
            get { return State.Get("PagerSuffix"); }
            set { State["PagerSuffix"] = value; }
        }

        public bool DisplayPager {
            get { return State.Get("DisplayPager").ToBoolean().GetValueOrDefault(); }
            set { State["DisplayPager"] = value.ToString(CultureInfo.InvariantCulture); }
        }
    }
}