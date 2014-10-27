using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Serialization;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Layouts.ViewModels;

namespace Orchard.Layouts.Drivers {
    public class LayoutPartDriver : ContentPartDriver<LayoutPart> {
        private readonly ILayoutSerializer _serializer;
        private readonly IElementDisplay _elementDisplay;
        private readonly IElementManager _elementManager;

        public LayoutPartDriver(ILayoutSerializer serializer, IElementDisplay elementDisplay, IElementManager elementManager) {
            _serializer = serializer;
            _elementDisplay = elementDisplay;
            _elementManager = elementManager;
        }

        protected override DriverResult Display(LayoutPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Layout", () => {
                var describeContext = new DescribeElementsContext { Content = part };
                var instances = _serializer.Deserialize(part.LayoutState, describeContext);
                var layoutRoot = _elementDisplay.DisplayElements(instances, part, displayType: displayType);
                return shapeHelper.Parts_Layout(LayoutRoot: layoutRoot);
            });
        }

        protected override DriverResult Editor(LayoutPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(LayoutPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_Layout_Edit", () => {
                var viewModel = new LayoutPartViewModel {
                    Part = part,
                    State = part.LayoutState,
                    TemplateId = part.TemplateId,
                };

                if (updater != null) {
                    updater.TryUpdateModel(viewModel, Prefix, null, new[] { "Part", "Templates" });
                    var describeContext = new DescribeElementsContext { Content = part };
                    var elementInstances = _serializer.Deserialize(viewModel.State, describeContext).ToArray();
                    var removedElementInstances = _serializer.Deserialize(viewModel.Trash, describeContext).ToArray();
                    var context = new LayoutSavingContext {
                        Content = part,
                        Updater = updater,
                        Elements = elementInstances,
                        RemovedElements = removedElementInstances
                    };
                    
                    _elementManager.Saving(context);
                    _elementManager.Removing(context);

                    part.LayoutState = _serializer.Serialize(elementInstances);
                    part.TemplateId = viewModel.TemplateId;
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.Layout", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override void Exporting(LayoutPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetElementValue("LayoutState", part.LayoutState);

            if (part.TemplateId != null) {
                var template = part.ContentItem.ContentManager.Get(part.TemplateId.Value);

                if (template != null) {
                    var templateIdentity = part.ContentItem.ContentManager.GetItemMetadata(template).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("TemplateId", templateIdentity);
                }
            }
        }

        protected override void Importing(LayoutPart part, ImportContentContext context) {
            part.LayoutState = context.Data.Element(part.PartDefinition.Name).El("LayoutState");
            context.ImportAttribute(part.PartDefinition.Name, "TemplateId", s => part.TemplateId = GetTemplateId(context, s));
        }

        private static int? GetTemplateId(ImportContentContext context, string templateIdentity) {
            if (String.IsNullOrWhiteSpace(templateIdentity))
                return null;

            var template = context.GetItemFromSession(templateIdentity);
            return template != null ? template.Id : default(int?);
        }
    }
}