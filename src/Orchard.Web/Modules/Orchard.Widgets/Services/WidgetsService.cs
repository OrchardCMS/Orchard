using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Widgets.Models;
using Orchard.Core.Common.Models;
using Orchard.Services;

namespace Orchard.Widgets.Services {

    public class WidgetsService : IWidgetsService {
        private readonly IFeatureManager _featureManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IContentManager _contentManager;
        private readonly IJsonConverter _jsonConverter;
        public WidgetsService(
            IContentManager contentManager,
            IFeatureManager featureManager,
            IExtensionManager extensionManager,
            IJsonConverter jsonConverter) {

            _contentManager = contentManager;
            _featureManager = featureManager;
            _extensionManager = extensionManager;
            _jsonConverter = jsonConverter;
        }

        public IEnumerable<Tuple<string, string>> GetWidgetTypes() {
            return _contentManager.GetContentTypeDefinitions()
                .Where(contentTypeDefinition => contentTypeDefinition.Settings.ContainsKey("Stereotype") && contentTypeDefinition.Settings["Stereotype"] == "Widget")
                .Select(contentTypeDefinition =>
                    Tuple.Create(
                        contentTypeDefinition.Name,
                        contentTypeDefinition.Settings.ContainsKey("Description") ? contentTypeDefinition.Settings["Description"] : null));
        }

        public IEnumerable<string> GetWidgetTypeNames() {
            return GetWidgetTypes().Select(type => type.Item1);
        }

        public IEnumerable<LayerPart> GetLayers() {
            return _contentManager
                .Query<LayerPart, LayerPartRecord>()
                .WithQueryHints(new QueryHints().ExpandParts<CommonPart>())
                .List();
        }

        public IEnumerable<WidgetPart> GetWidgets() {
            return _contentManager
                .Query<WidgetPart, WidgetPartRecord>()
                .ForVersion(VersionOptions.Latest)
                .WithQueryHints(new QueryHints().ExpandParts<CommonPart>())
                .List();
        }

        public IEnumerable<WidgetPart> GetOrphanedWidgets() {
            return _contentManager
                .Query<WidgetPart, WidgetPartRecord>()
                .ForVersion(VersionOptions.Latest)
                .WithQueryHints(new QueryHints().ExpandParts<CommonPart>())
                .Where<CommonPartRecord>(x => x.Container == null)
                .List();
        }

        public IEnumerable<WidgetPart> GetWidgets(int layerId) {
            return _contentManager
                .Query<WidgetPart, WidgetPartRecord>()
                .ForVersion(VersionOptions.Latest)
                .WithQueryHints(new QueryHints().ExpandParts<CommonPart>())
                .Where<CommonPartRecord>(x => x.Container.Id == layerId)
                .List();
        }

        public IEnumerable<WidgetPart> GetWidgets(int[] layerIds) {
            return _contentManager
                .Query<WidgetPart, WidgetPartRecord>()
                .WithQueryHints(new QueryHints().ExpandParts<CommonPart>())
                .Where<CommonPartRecord>(x => layerIds.Contains(x.Container.Id))
                .List();
        }

        public bool HasDedicatedZones(ExtensionDescriptor theme, string layer) {
            if (theme == null) {
                return false;
            }
            IEnumerable<string> layers = string.IsNullOrEmpty(theme.Layers) ? Enumerable.Empty<string>() : theme.Layers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(layer)
                && !string.IsNullOrEmpty(theme.Layers)
                && layers.Contains(layer, StringComparer.InvariantCultureIgnoreCase)) {
                Dictionary<string, string> layerZones = _jsonConverter.Deserialize<Dictionary<string, string>>(theme.LayerZones);
                if (layerZones.ContainsKey(layer))
                    return true;
            }
            return false;
        }

        private IEnumerable<string> GetAllThemeZones(ExtensionDescriptor theme) {
            if (theme == null) {
                return Enumerable.Empty<string>();
            }
            IEnumerable<string> zones = new List<string>();
            IEnumerable<string> layers = string.IsNullOrEmpty(theme.Layers) ? Enumerable.Empty<string>() : theme.Layers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(theme.Layers)) {
                Dictionary<string, string> layerZones = _jsonConverter.Deserialize<Dictionary<string, string>>(theme.LayerZones);
                foreach( var lz in layerZones.Keys){
                    var value = layerZones[lz];
                    zones = zones.Concat(value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                                        .Select(x => x.Trim())
                                                                                        .Distinct());
                }
            }
            // get the zones for this theme
            if (theme.Zones != null)
                zones = zones.Concat(theme.Zones.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                            .Select(x => x.Trim())
                                                                            .Distinct());
            return zones.Distinct();
        }

        public IEnumerable<string> GetZones() {
            IEnumerable<string> zones = new List<string>(); 
            var themes = _featureManager.GetEnabledFeatures()
                .Select(x => x.Extension)
                .Where(x => DefaultExtensionTypes.IsTheme(x.ExtensionType));
            foreach (var theme in themes) {
                var zn = GetAllThemeZones(theme);
                if (zn.Any())
                    zones = zn.Concat(zones);
            }
            return zones.Distinct();
        }

        private IEnumerable<string> GetThemeZones(ExtensionDescriptor theme, string layer) {
            if (theme == null) {
                return Enumerable.Empty<string>();
            }
            IEnumerable<string> zones = new List<string>();
            IEnumerable<string> layers = string.IsNullOrEmpty(theme.Layers) ? Enumerable.Empty<string>() : theme.Layers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(layer)
                && !string.IsNullOrEmpty(theme.Layers)
                && layers.Contains(layer, StringComparer.InvariantCultureIgnoreCase)) {
                Dictionary<string, string> layerZones = _jsonConverter.Deserialize<Dictionary<string, string>>(theme.LayerZones);
                if (layerZones.ContainsKey(layer)) {
                    var value = layerZones[layer];
                    zones = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Distinct()
                        .ToList();
                }
            }
            else if (string.IsNullOrEmpty(layer)
                || string.IsNullOrEmpty(theme.Layers)
                || !layers.Contains(layer, StringComparer.InvariantCultureIgnoreCase)) {
                // get the zones for this theme
                if (theme.Zones != null)
                    zones = theme.Zones.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Distinct()
                        .ToList();
            }
            return zones;
        }

        public IEnumerable<string> GetZones(ExtensionDescriptor theme) {
            return GetZones(theme, null);
        }

        public IEnumerable<string> GetZones(ExtensionDescriptor theme,string layer=null) {
            if(theme == null) {
                return Enumerable.Empty<string>();
            }
            string baseTheme = theme.BaseTheme;
            IEnumerable<string> zones = new List<string>();
            // if this theme has no zones defined then walk the BaseTheme chain until we hit a theme which defines zones
            do {
                zones = GetThemeZones(theme, layer);
                if (!zones.Any()) {
                    if (!string.IsNullOrEmpty(baseTheme)) {
                        theme = _extensionManager.GetExtension(baseTheme);
                        baseTheme = theme != null ? theme.BaseTheme : null;
                    }
                    else theme = null;
                }
            } while (!zones.Any() && theme != null);
            return zones.Distinct();
        }

        public LayerPart GetLayer(int layerId) {
            return GetLayers().FirstOrDefault(layer => layer.Id == layerId);
        }

        public LayerPart CreateLayer(string name, string description, string layerRule) {
            LayerPart layerPart = _contentManager.Create<LayerPart>("Layer",
                layer => {
                    layer.Name = name;
                    layer.Description = description;
                    layer.LayerRule = layerRule;
                });

            return layerPart;
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
                .ForVersion(VersionOptions.Latest)
                .Where(widget => widget.Id == widgetId)
                .List()
                .FirstOrDefault();
        }

        public WidgetPart CreateWidget(int layerId, string widgetType, string title, string position, string zone) {
            LayerPart layerPart = GetLayer(layerId);

            WidgetPart widgetPart = _contentManager.Create<WidgetPart>(widgetType,
                VersionOptions.Draft,
                widget => {
                    widget.Title = title;
                    widget.Position = position;
                    widget.Zone = zone;
                    widget.LayerPart = layerPart;
                });

            return widgetPart;
        }

        public void DeleteWidget(int widgetId) {
            _contentManager.Remove(GetWidget(widgetId).ContentItem);
        }

        public bool MoveWidgetUp(int widgetId) {
            return MoveWidgetUp(GetWidget(widgetId));
        }
        public bool MoveWidgetUp(WidgetPart widgetPart) {
            int currentPosition = ParsePosition(widgetPart);

            WidgetPart widgetBefore = GetWidgets()
                .Where(widget => widget.Zone == widgetPart.Zone)
                .OrderByDescending(widget => widget.Position, new UI.FlatPositionComparer())
                .FirstOrDefault(widget => ParsePosition(widget) < currentPosition);

            if (widgetBefore != null) {
                widgetPart.Position = widgetBefore.Position;
                MakeRoomForWidgetPosition(widgetPart);
                return true;
            }

            return false;
        }

        public bool MoveWidgetDown(int widgetId) {
            return MoveWidgetDown(GetWidget(widgetId));
        }
        public bool MoveWidgetDown(WidgetPart widgetPart) {
            int currentPosition = ParsePosition(widgetPart);

            WidgetPart widgetAfter = GetWidgets()
                .Where(widget => widget.Zone == widgetPart.Zone)
                .OrderBy(widget => widget.Position, new UI.FlatPositionComparer())
                .FirstOrDefault(widget => ParsePosition(widget) > currentPosition);

            if (widgetAfter != null) {
                widgetAfter.Position = widgetPart.Position;
                MakeRoomForWidgetPosition(widgetAfter);
                return true;
            }

            return false;
        }

        public bool MoveWidgetToLayer(int widgetId, int? layerId) {
            return MoveWidgetToLayer(GetWidget(widgetId), layerId);
        }
        public bool MoveWidgetToLayer(WidgetPart widgetPart, int? layerId) {
            LayerPart layer = layerId.HasValue
                ? GetLayer(layerId.Value)
                : GetLayers().FirstOrDefault();

            if (layer != null) {
                widgetPart.LayerPart = layer;
                return true;
            }

            return false;
        }

        public void MakeRoomForWidgetPosition(int widgetId) {
            MakeRoomForWidgetPosition(GetWidget(widgetId));
        }
        public void MakeRoomForWidgetPosition(WidgetPart widgetPart) {
            int targetPosition = ParsePosition(widgetPart);

            IEnumerable<WidgetPart> widgetsToMove = GetWidgets()
                .Where(widget => widget.Zone == widgetPart.Zone && ParsePosition(widget) >= targetPosition && widget.Id != widgetPart.Id)
                .OrderBy(widget => widget.Position, new UI.FlatPositionComparer()).ToList();

            // no need to continue if there are no widgets that will conflict with this widget's position
            if (!widgetsToMove.Any() || ParsePosition(widgetsToMove.First()) > targetPosition)
                return;

            int position = targetPosition;
            foreach (WidgetPart widget in widgetsToMove)
                widget.Position = (++position).ToString();
        }

        private static int ParsePosition(WidgetPart widgetPart) {
            int value;
            if (!int.TryParse(widgetPart.Position, out value))
                return 0;
            return value;
        }
    }
}