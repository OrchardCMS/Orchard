using System.Web.Mvc;

namespace Orchard.UI.Zones {
#if REFACTORING
    public interface IZoneManager : IDependency {
        void Render<TModel>(HtmlHelper<TModel> html, ZoneCollection zones, string zoneName, string partitions, string[] except);
    }
#endif
}