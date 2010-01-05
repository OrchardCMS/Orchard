using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Orchard.UI.Zones {
    public class ContentPartEditorZoneItem : ZoneItem {
        public object Model { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }

        public override void Execute<TModel>(HtmlHelper<TModel> html) {
            html.ViewContext.Writer.Write(
                html.EditorFor(m => Model, TemplateName, Prefix));
        }
    }
}