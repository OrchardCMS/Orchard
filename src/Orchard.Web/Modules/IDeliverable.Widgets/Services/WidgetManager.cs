using System.Collections.Generic;
using System.Linq;
using IDeliverable.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace IDeliverable.Widgets.Services
{
    [OrchardFeature("IDeliverable.Widgets")]
    public class WidgetManager : IWidgetManager
    {
        private readonly IContentManager _contentManager;
        private readonly IWidgetsService _widgetsService;

        public WidgetManager(IContentManager contentManager, IWidgetsService widgetsService)
        {
            _contentManager = contentManager;
            _widgetsService = widgetsService;
        }

        public IEnumerable<WidgetExPart> GetWidgets(int hostId, VersionOptions versionOptions = null)
        {
            versionOptions = versionOptions ?? VersionOptions.Published;
            return _contentManager
                .Query<WidgetExPart, WidgetExPartRecord>()
                .ForVersion(versionOptions)
                .Where(x => x.HostId == hostId)
                .List();
        }

        public LayerPart GetContentLayer()
        {
            var contentLayer = _widgetsService.GetLayers().FirstOrDefault(x => x.Name == "ContentWidgets")
                ?? _widgetsService.CreateLayer("ContentWidgets", "This layer never activates, but is needed for the widgets hosted by content items for now.", "false");

            return contentLayer;
        }
    }
}