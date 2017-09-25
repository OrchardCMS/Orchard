using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Services {
    public interface IWidgetsService : IDependency {
        
        IEnumerable<string> GetZones();
        IEnumerable<string> GetZones(ExtensionDescriptor theme);

        IEnumerable<LayerPart> GetLayers();

        IEnumerable<Tuple<string, string>> GetWidgetTypes();
        IEnumerable<string> GetWidgetTypeNames();
        IEnumerable<WidgetPart> GetWidgets();
        IEnumerable<WidgetPart> GetOrphanedWidgets();
        IEnumerable<WidgetPart> GetWidgets(int layerId);
        IEnumerable<WidgetPart> GetWidgets(int[] layerIds);

        WidgetPart GetWidget(int widgetId);
        WidgetPart CreateWidget(int layerId, string widgetType, string title, string position, string zone);
        void DeleteWidget(int widgetId);

        LayerPart GetLayer(int layerId);
        LayerPart CreateLayer(string name, string description, string layerRule);
        void DeleteLayer(int layerId);

        bool MoveWidgetUp(int widgetId);
        bool MoveWidgetUp(WidgetPart widgetPart);
        bool MoveWidgetDown(int widgetId);
        bool MoveWidgetDown(WidgetPart widgetPart);
        bool MoveWidgetToLayer(int widgetId, int? layerId);
        bool MoveWidgetToLayer(WidgetPart widgetPart, int? layerId);
        void MakeRoomForWidgetPosition(int widgetId);
        void MakeRoomForWidgetPosition(WidgetPart widgetPart);
    }
}