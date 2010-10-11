using System.Collections.Generic;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.ViewModels
{
    public class WidgetsIndexViewModel
    {
        /// <summary>
        /// The available widget content types.
        /// </summary>
        public IEnumerable<string> WidgetTypes { get; set; }

        /// <summary>
        /// The available layers.
        /// </summary>
        public IEnumerable<LayerPart> Layers { get; set; }

        /// <summary>
        /// The available zones in the page.
        /// </summary>
        public IEnumerable<string> Zones { get; set; }

        /// <summary>
        /// The current layer.
        /// </summary>
        public LayerPart CurrentLayer { get; set; }

        /// <summary>
        /// The current layer widgets.
        /// </summary>
        public IEnumerable<WidgetPart> CurrentLayerWidgets { get; set; }
    }
}