using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Widgets.Models {
    public class WidgetPart : ContentPart<WidgetPartRecord>, ITitleAspect {

        /// <summary>
        /// The widget's title.
        /// </summary>
        public string Title {
            get { return Retrieve(x => x.Title); }
            set { Store(x => x.Title, value); }
        }

        /// <summary>
        /// The zone where the widget is to be displayed.
        /// </summary>
        [Required]
        public string Zone {
            get { return Retrieve(x => x.Zone); }
            set { Store(x => x.Zone, value); }
        }

        /// <summary>
        /// Whether or not the Title should be rendered on the front-end.
        /// </summary>
        public bool RenderTitle {
            get { return Retrieve(x => x.RenderTitle); }
            set { Store(x => x.RenderTitle, value); }
        }

        /// <summary>
        /// The widget's position within the zone.
        /// </summary>
        [Required]
        public string Position {
            get { return Retrieve(x => x.Position); }
            set { Store(x => x.Position, value); }
        }

        /// <summary>
        /// The technical name of the widget.
        /// </summary>
        public string Name {
            get { return Retrieve(x => x.Name); }
            set { Store(x => x.Name, value); }
        }

        /// <summary>
        /// The layerPart where the widget belongs.
        /// </summary>
        public LayerPart LayerPart {
            get { return this.As<ICommonPart>().Container.As<LayerPart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        /// <summary>
        /// The layerPart identifier.
        /// </summary>
        public int? LayerId {
            get {
                var layerPart = LayerPart;
                return layerPart != null ? layerPart.Id : default(int?);
            }
        }

        /// <summary>
        /// The available page zones.
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public IEnumerable<string> AvailableZones { get; set; }

        /// <summary>
        /// The available layers.
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public IEnumerable<LayerPart> AvailableLayers { get; set; }

        /// <summary>
        /// Css classes for the widget.
        /// </summary>
        public string CssClasses {
            get { return this.Retrieve(x => x.CssClasses); }
            set { this.Store(x => x.CssClasses, value); }
        }
    }
}