using System;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class Projection : Element {

        public override string Category {
            get { return "Content"; }
        }

        public string QueryLayoutId {
            get { return ElementDataHelper.Retrieve(this, x => x.QueryLayoutId); }
            set { this.Store(x => x.QueryLayoutId, value); }
        }

        public int? QueryId {
            get {
                return String.IsNullOrWhiteSpace(QueryLayoutId) ? null : XmlHelper.Parse<int?>(QueryLayoutId.Split(new[] { ';' })[0]);
            }
            set {
                QueryLayoutId = String.Format("{0};{1}", value, LayoutId);
            }
        }

        public int? LayoutId {
            get {
                return String.IsNullOrWhiteSpace(QueryLayoutId) ? null : XmlHelper.Parse<int?>(QueryLayoutId.Split(new[] { ';' })[1]);
            }
            set {
                QueryLayoutId = String.Format("{0};{1}", QueryId, value);
            }
        }

        public int ItemsToDisplay {
            get { return ElementDataHelper.Retrieve(this, x => x.ItemsToDisplay); }
            set { this.Store(x => x.ItemsToDisplay, value); }
        }

        public int Skip {
            get { return ElementDataHelper.Retrieve(this, x => x.Skip); }
            set { this.Store(x => x.Skip, value); }
        }

        public int MaxItems {
            get { return ElementDataHelper.Retrieve(this, x => x.MaxItems); }
            set { this.Store(x => x.MaxItems, value); }
        }

        public string PagerSuffix {
            get { return ElementDataHelper.Retrieve(this, x => x.PagerSuffix); }
            set { this.Store(x => x.PagerSuffix, value); }
        }

        public bool DisplayPager {
            get { return ElementDataHelper.Retrieve(this, x => x.DisplayPager); }
            set { this.Store(x => x.DisplayPager, value); }
        }
    }
}