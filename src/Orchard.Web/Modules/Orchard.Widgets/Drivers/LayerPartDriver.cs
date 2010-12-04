using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Drivers {

    [UsedImplicitly]
    public class LayerPartDriver : ContentPartDriver<LayerPart> {
        private readonly IRuleManager _ruleManager;
        private readonly IWidgetsService _widgetsService;

        public LayerPartDriver(
            IRuleManager ruleManager,
            IWidgetsService widgetsService) {

            _ruleManager = ruleManager;
            _widgetsService = widgetsService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(LayerPart layerPart, dynamic shapeHelper) {
            return ContentShape("Parts_Widgets_LayerPart",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Widgets.LayerPart", Model: layerPart, Prefix: Prefix));
        }

        protected override DriverResult Editor(LayerPart layerPart, IUpdateModel updater, dynamic shapeHelper) {
            if(updater.TryUpdateModel(layerPart, Prefix, null, null)) {
                if (String.IsNullOrWhiteSpace(layerPart.LayerRule)) {
                    layerPart.LayerRule = "true";
                }

                if (_widgetsService.GetLayers()
                    .Any(l => 
                        l.Id != layerPart.Id
                        && String.Equals(l.Name, layerPart.Name, StringComparison.InvariantCultureIgnoreCase))) { 
                    updater.AddModelError("Name", T("A Layer with the same name already exists"));
                }

                try {
                    _ruleManager.Matches(layerPart.LayerRule);
                }
                catch (Exception e) {
                    updater.AddModelError("Description", T("The rule is not valid: {0}", e.Message));
                }
            }

            return Editor(layerPart, shapeHelper);
        }
    }
}