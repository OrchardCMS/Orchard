using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Orchard.UI.Zones {
#if REFACTORING
    public class ContentPartDisplayZoneItem : ZoneItem {
        public object Model { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }

        public override void Execute<TModel>(HtmlHelper<TModel> html) {
            var htmlString = html.DisplayFor(m => Model, TemplateName, Prefix);
            html.ViewContext.Writer.Write(htmlString);
        }
    }
#endif
}