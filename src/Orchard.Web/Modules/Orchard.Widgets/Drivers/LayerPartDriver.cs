using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Conditions.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Drivers {

    public class LayerPartDriver : ContentPartDriver<LayerPart> {
        private readonly IConditionManager _conditionManager;
        private readonly IWidgetsService _widgetsService;

        public LayerPartDriver(
            IConditionManager conditionManager,
            IWidgetsService widgetsService) {

            _conditionManager = conditionManager;
            _widgetsService = widgetsService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(LayerPart layerPart, dynamic shapeHelper) {
            var results = new List<DriverResult> {
                ContentShape("Parts_Widgets_LayerPart",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts.Widgets.LayerPart", Model: layerPart, Prefix: Prefix))
            };

            if (layerPart.Id > 0)
                results.Add(ContentShape("Widget_DeleteButton",
                    deleteButton => deleteButton));

            return Combined(results.ToArray());
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
                    _conditionManager.Matches(layerPart.LayerRule);
                }
                catch (Exception e) {
                    updater.AddModelError("Description", T("The rule is not valid: {0}", e.Message));
                }
            }

            return Editor(layerPart, shapeHelper);
        }

        protected override void Importing(LayerPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            var name = context.Attribute(part.PartDefinition.Name, "Name");
            if (name != null) {
                part.Name = name;
            }

            var description = context.Attribute(part.PartDefinition.Name, "Description");
            if (description != null) {
                part.Description = description;
            }

            var rule = context.Attribute(part.PartDefinition.Name, "LayerRule");
            if (rule != null) {
                part.LayerRule = rule;    
            }
        }

        protected override void Exporting(LayerPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Name", part.Name);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Description", part.Description);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LayerRule", part.LayerRule);
        }
    }
}