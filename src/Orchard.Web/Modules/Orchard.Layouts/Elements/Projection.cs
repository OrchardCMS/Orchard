using System;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class Projection : Element {

        public override string Category {
            get { return "Content"; }
        }
        
        public string QueryLayoutAlias {
            get { return ElementDataHelper.Retrieve(this, x => x.QueryLayoutAlias); }
            set { this.Store(x => x.QueryLayoutAlias, value); }
        }

        public int? QueryId {
            get {
                return String.IsNullOrWhiteSpace(QueryLayoutAlias) ? null : XmlHelper.Parse<int?>(QueryLayoutAlias.Split(new[] { ';' })[0]);
            }
            set {
                QueryLayoutAlias = String.Format("{0};{1}", value, LayoutAlias);
            }
        }

        public string LayoutAlias {
            get {
                return QueryLayoutAlias.Split(new[] { ';' })[1];                
            }
            set {
                QueryLayoutAlias = String.Format("{0};{1}", QueryId, value);
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