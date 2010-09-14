using System.Web.Mvc;
using Orchard.Mvc.Html;
using Orchard.Mvc.ViewModels;

namespace Orchard.UI.Zones {
#if REFACTORING
    public class ContentItemDisplayZoneItem : ZoneItem {
        public ContentItemViewModel ViewModel { get; set; }

        public override void Execute<TModel>(HtmlHelper<TModel> html) {
            var htmlString = html.DisplayForItem(ViewModel);
            html.ViewContext.Writer.Write(htmlString);
        }
    }
#endif
}