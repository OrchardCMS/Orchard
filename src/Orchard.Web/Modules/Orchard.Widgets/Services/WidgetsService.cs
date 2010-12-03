using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Services {

    [UsedImplicitly]
    public class WidgetsService : IWidgetsService {
        private readonly IFeatureManager _featureManager;
        private readonly IContentManager _contentManager;

        public WidgetsService(
            IContentManager contentManager,
            IFeatureManager featureManager) {

            _contentManager = contentManager;
            _featureManager = featureManager;
        }

        public IEnumerable<string> GetWidgetTypes() {
            return _contentManager.GetContentTypeDefinitions()
                .Where(contentTypeDefinition => contentTypeDefinition.Settings.ContainsKey("Stereotype") && contentTypeDefinition.Settings["Stereotype"] == "Widget")
                .Select(contentTypeDefinition => contentTypeDefinition.Name);
        }

        public IEnumerable<LayerPart> GetLayers() {
            return _contentManager
                .Query<LayerPart, LayerPartRecord>()
                .List();
        }

        public IEnumerable<WidgetPart> GetWidgets() {
            return _contentManager
                .Query<WidgetPart, WidgetPartRecord>()
                .List();
        }

        public IEnumerable<string> GetZones() {
            return _featureManager.GetEnabledFeatures()
                .Select(x => x.Extension)
                .Where(x => DefaultExtensionTypes.IsTheme(x.ExtensionType))
                .SelectMany(x => x.Zones.Split(','))
                .Distinct()
                .Select(x => x.Trim())
                .ToArray();
        }

        public IEnumerable<WidgetPart> GetWidgets(int layerId) {
            return GetWidgets().Where(widgetPart => widgetPart.As<ICommonPart>().Container.ContentItem.Id == layerId);
        }

        public LayerPart GetLayer(int layerId) {
            return GetLayers().FirstOrDefault(layer => layer.Id == layerId);
        }

        public LayerPart CreateLayer(string name, string description, string layerRule) {
            LayerPart layerPart = _contentManager.Create<LayerPart>("Layer",
                layer => {
                    layer.Record.Name = name;
                    layer.Record.Description = description;
                    layer.Record.LayerRule = layerRule;
                });

            return layerPart;
        }

        public void UpdateLayer(int layerId, string name, string description, string layerRule) {
            LayerPart layerPart = GetLayer(layerId);
            layerPart.Record.Name = name;
            layerPart.Record.Description = description;
            layerPart.Record.LayerRule = layerRule;
        }

        public void DeleteLayer(int layerId) {
            // Delete widgets in the layer
            foreach (WidgetPart widgetPart in GetWidgets(layerId)) {
                DeleteWidget(widgetPart.Id);
            }

            // Delete actual layer
            _contentManager.Remove(GetLayer(layerId).ContentItem);
        }

        public WidgetPart GetWidget(int widgetId) {
            return _contentManager
                .Query<WidgetPart, WidgetPartRecord>()
                .Where(widget => widget.Id == widgetId)
                .List()
                .FirstOrDefault();
        }

        public WidgetPart CreateWidget(int layerId, string widgetType, string title, string position, string zone) {
            LayerPart layerPart = GetLayer(layerId);

            WidgetPart widgetPart = _contentManager.Create<WidgetPart>(widgetType,
                widget => {
                    widget.Record.Title = title;
                    widget.Record.Position = position;
                    widget.Record.Zone = zone;
                    widget.LayerPart = layerPart;
                });

            return widgetPart;
        }

        public void UpdateWidget(int widgetId, string title, string position, string zone) {
            WidgetPart widgetPart = GetWidget(widgetId);
            widgetPart.Record.Title = title;
            widgetPart.Record.Position = position;
            widgetPart.Record.Zone = zone;
        }

        public void DeleteWidget(int widgetId) {
            _contentManager.Remove(GetWidget(widgetId).ContentItem);
        }

        public bool MoveWidgetUp(WidgetPart widgetPart) {
            int currentPosition = ParsePosition(widgetPart);

            WidgetPart widgetBefore = GetWidgets(widgetPart.LayerPart.Id)
                .Where(widget => widget.Zone == widgetPart.Zone)
                .OrderByDescending(widget => widget.Position, new UI.FlatPositionComparer())
                .FirstOrDefault(widget => ParsePosition(widget) < currentPosition);

            if (widgetBefore != null) {
                SwitchWidgetPositions(widgetBefore, widgetPart);

                return true;
            }

            return false;
        }

        private int ParsePosition(WidgetPart widgetPart) {
            int value;
            if (!int.TryParse(widgetPart.Record.Position, out value))
                return 0;
            return value;
        }

        public bool MoveWidgetUp(int widgetId) {
            return MoveWidgetUp(GetWidget(widgetId));
        }

        public bool MoveWidgetDown(WidgetPart widgetPart) {
            int currentPosition = ParsePosition(widgetPart);

            WidgetPart widgetAfter = GetWidgets(widgetPart.LayerPart.Id)
                .Where(widget => widget.Zone == widgetPart.Zone)
                .OrderBy(widget => widget.Position, new UI.FlatPositionComparer())
                .FirstOrDefault(widget => ParsePosition(widget) > currentPosition);

            if (widgetAfter != null) {
                SwitchWidgetPositions(widgetAfter, widgetPart);

                return true;
            }

            return false;
        }

        public bool MoveWidgetDown(int widgetId) {
            return MoveWidgetDown(GetWidget(widgetId));
        }

        private static void SwitchWidgetPositions(WidgetPart sourceWidget, WidgetPart targetWidget) {
            string tempPosition = sourceWidget.Record.Position;
            sourceWidget.Record.Position = targetWidget.Record.Position;
            targetWidget.Record.Position = tempPosition;
        }
    }
}