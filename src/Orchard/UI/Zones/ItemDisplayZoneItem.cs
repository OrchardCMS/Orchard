using System.Web.Mvc;
using Orchard.Mvc.Html;
using Orchard.Mvc.ViewModels;

namespace Orchard.UI.Zones {
    public class ItemDisplayZoneItem : ZoneItem {
        public ItemViewModel ViewModel { get; set; }

        public override void Execute<TModel>(HtmlHelper<TModel> html) {
            html.DisplayForItem(ViewModel);
        }
    }
}