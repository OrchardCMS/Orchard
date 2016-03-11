using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Utility.Extensions;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Drivers {

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

            if (!String.IsNullOrEmpty(widgetPart.CssClasses)) {
                var classNames = widgetPart.CssClasses.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < classNames.Length; i++) {
                    classNames[i] = classNames[i].Trim().HtmlClassify();
                }

                widgetPart.CssClasses = String.Join(" ", classNames);
            }

            _widgetsService.MakeRoomForWidgetPosition(widgetPart);

            return Editor(widgetPart, shapeHelper);
        }

        protected override void Importing(WidgetPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Title", x => part.Title = x);
            context.ImportAttribute(part.PartDefinition.Name, "Position", x => part.Position = x);
            context.ImportAttribute(part.PartDefinition.Name, "Zone", x => part.Zone = x);
            context.ImportAttribute(part.PartDefinition.Name, "RenderTitle", x => {
                if (!string.IsNullOrWhiteSpace(x)) {
                    part.RenderTitle = Convert.ToBoolean(x);
                }
            });
            context.ImportAttribute(part.PartDefinition.Name, "Name", x => part.Name = x);
            context.ImportAttribute(part.PartDefinition.Name, "Title", x => {
                if (!String.IsNullOrWhiteSpace(x))
                    part.Title = x;
            });
            context.ImportAttribute(part.PartDefinition.Name, "CssClasses", x => part.CssClasses = x);
        }

        protected override void Exporting(WidgetPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Title", part.Title);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Position", part.Position);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Zone", part.Zone);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RenderTitle", part.RenderTitle);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Name", part.Name);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CssClasses", part.CssClasses);
        }
    }
}
