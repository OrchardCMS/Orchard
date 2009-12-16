using System.Web.Mvc;

namespace Orchard.UI.Zones {
    public abstract class ZoneItem {
        public string Position { get; set; }
        public bool WasExecuted { get; set; }

        public abstract void Execute<TModel>(HtmlHelper<TModel> html);
    }
}