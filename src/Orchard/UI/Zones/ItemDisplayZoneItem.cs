using System.Web.Mvc;
using Orchard.Models.ViewModels;
using Orchard.Mvc.Html;

namespace Orchard.UI.Zones {
    public class ItemDisplayZoneItem : ZoneItem {
        public ItemDisplayModel DisplayModel { get; set; }

        public override void Execute<TModel>(HtmlHelper<TModel> html) {
            html.DisplayForItem(DisplayModel);
        }
    }
}