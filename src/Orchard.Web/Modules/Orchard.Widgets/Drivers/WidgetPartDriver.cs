using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Utility.Extensions;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Drivers {

    [UsedImplicitly]
    public class WidgetPartDriver : ContentPartDriver<WidgetPart> {
        private readonly IWidgetsService _widgetsService;
        private readonly IContentManager _contentManager;

        public WidgetPartDriver(IWidgetsService widgetsService, IContentManager contentManager) {
            _widgetsService = widgetsService;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "WidgetPart"; }
        }

        protected override DriverResult Editor(WidgetPart widgetPart, dynamic shapeHelper) {
            widgetPart.AvailableZones = _widgetsService.GetZones();
            widgetPart.AvailableLayers = _widgetsService.GetLayers();

            var results = new List<DriverResult> {
                ContentShape("Parts_Widgets_WidgetPart",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts.Widgets.WidgetPart", Model: widgetPart, Prefix: Prefix))
            };

            if (widgetPart.Id > 0)
                results.Add(ContentShape("Widget_DeleteButton",
                    deleteButton => deleteButton));

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(WidgetPart widgetPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(widgetPart, Prefix, null, null);

            if(string.IsNullOrWhiteSpace(widgetPart.Title)) {
                updater.AddModelError("Title", T("Title can't be empty."));
            }
            
            // if there is a name, ensure it's unique
            if(!string.IsNullOrWhiteSpace(widgetPart.Name)) {
                widgetPart.Name = widgetPart.Name.ToHtmlName();

                var widgets = _contentManager.Query<WidgetPart, WidgetPartRecord>().Where(x => x.Name == widgetPart.Name && x.Id != widgetPart.Id).Count();
                if(widgets > 0) {
                    updater.AddModelError("Name", T("A Widget with the same Name already exists."));
                }
            }

            _widgetsService.MakeRoomForWidgetPosition(widgetPart);

            return Editor(widgetPart, shapeHelper);
        }

        protected override void Importing(WidgetPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            var title = context.Attribute(part.PartDefinition.Name, "Title");
            if (title != null) {
                part.Title = title;
            }

            var position = context.Attribute(part.PartDefinition.Name, "Position");
            if (position != null) {
                part.Position = position;
            }

            var zone = context.Attribute(part.PartDefinition.Name, "Zone");
            if (zone != null) {
                part.Zone = zone;
            }

            var renderTitle = context.Attribute(part.PartDefinition.Name, "RenderTitle");
            if (!string.IsNullOrWhiteSpace(renderTitle)) {
                part.RenderTitle = Convert.ToBoolean(renderTitle);
            }

            var name = context.Attribute(part.PartDefinition.Name, "Name");
            if (name != null) {
                part.Name = name;
            }
        }

        protected override void Exporting(WidgetPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Title", part.Title);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Position", part.Position);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Zone", part.Zone);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RenderTitle", part.RenderTitle);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Name", part.Name);
        }
    }
}