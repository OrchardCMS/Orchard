using System.Linq;
using System.Xml;
using IDeliverable.Widgets.Models;
using IDeliverable.Widgets.Services;
using IDeliverable.Widgets.ViewModels;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Themes.Services;
using Orchard.Widgets.Services;

namespace IDeliverable.Widgets.Drivers
{
    [OrchardFeature("IDeliverable.Widgets")]
    public class WidgetsContainerPartDriver : ContentPartDriver<WidgetsContainerPart>
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IWidgetsService _widgetsService;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IWidgetManager _widgetManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IContentManager _contentManager;

        public WidgetsContainerPartDriver(
            ISiteThemeService siteThemeService,
            IWidgetsService widgetsService,
            IVirtualPathProvider virtualPathProvider,
            IShapeFactory shapeFactory,
            IWidgetManager widgetManager,
            IWorkContextAccessor wca,
            IContentManager contentManager)
        {

            _siteThemeService = siteThemeService;
            _widgetsService = widgetsService;
            _virtualPathProvider = virtualPathProvider;
            New = shapeFactory;
            _widgetManager = widgetManager;
            _wca = wca;
            _contentManager = contentManager;
        }

        private dynamic New { get; set; }

        protected override string Prefix
        {
            get { return "WidgetsContainer"; }
        }

        protected override DriverResult Display(WidgetsContainerPart part, string displayType, dynamic shapeHelper)
        {
            // TODO: make DisplayType configurable
            if (displayType != "Detail")
                return null;

            var widgetParts = _widgetManager.GetWidgets(part.Id);

            // Build and add shape to zone.
            var workContext = _wca.GetContext();
            var zones = workContext.Layout.Zones;
            foreach (var widgetPart in widgetParts)
            {
                var widgetShape = _contentManager.BuildDisplay(widgetPart);
                zones[widgetPart.Zone].Add(widgetShape, widgetPart.Position);
            }

            return null;
        }

        protected override DriverResult Editor(WidgetsContainerPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_WidgetsContainer", () =>
            {
                var currentTheme = _siteThemeService.GetSiteTheme();
                var currentThemesZones = _widgetsService.GetZones(currentTheme).ToList();
                var widgetTypes = _widgetsService.GetWidgetTypeNames().ToList();
                var widgets = _widgetManager.GetWidgets(part.Id, VersionOptions.Latest);
                var zonePreviewImagePath = string.Format("{0}/{1}/ThemeZonePreview.png", currentTheme.Location, currentTheme.Id);
                var zonePreviewImage = _virtualPathProvider.FileExists(zonePreviewImagePath) ? zonePreviewImagePath : null;
                var layer = _widgetsService.GetLayers().First();

                var viewModel = New.ViewModel()
                    .CurrentTheme(currentTheme)
                    .Zones(currentThemesZones)
                    .ZonePreviewImage(zonePreviewImage)
                    .WidgetTypes(widgetTypes)
                    .Widgets(widgets)
                    .ContentItem(part.ContentItem)
                    .LayerId(layer.Id);

                return shapeHelper.EditorTemplate(TemplateName: "Parts.WidgetsContainer", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(WidgetsContainerPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var viewModel = new WidgetsContainerViewModel();
            if (updater.TryUpdateModel(viewModel, null, null, null))
            {
                UpdatePositions(viewModel);
                RemoveWidgets(viewModel);
            }

            return Editor(part, shapeHelper);
        }

        private void RemoveWidgets(WidgetsContainerViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.RemovedWidgets))
                return;

            var widgetIds = JsonConvert.DeserializeObject<int[]>(viewModel.RemovedWidgets);

            foreach (var widgetId in widgetIds)
            {
                _widgetsService.DeleteWidget(widgetId);
            }
        }

        private void UpdatePositions(WidgetsContainerViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.WidgetPlacement))
                return;

            var data = JsonConvert.DeserializeXNode(viewModel.WidgetPlacement);
            var zonesNode = data.Root;

            foreach (var zoneNode in zonesNode.Elements())
            {
                var zoneName = zoneNode.Name.ToString();
                var widgetElements = zoneNode.Elements("widgets");
                var position = 0;

                foreach (var widget in widgetElements
                    .Select(widgetNode => XmlConvert.ToInt32(widgetNode.Value))
                    .Select(widgetId => _widgetsService.GetWidget(widgetId))
                    .Where(widget => widget != null))
                {

                    widget.Position = (position++).ToString();
                    widget.Zone = zoneName;
                }
            }
        }
    }
}