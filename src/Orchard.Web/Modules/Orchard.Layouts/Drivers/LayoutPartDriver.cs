using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Layouts.ViewModels;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Drivers {
    public class LayoutPartDriver : ContentPartDriver<LayoutPart> {
        private readonly ILayoutSerializer _serializer;
        private readonly IElementDisplay _elementDisplay;
        private readonly IElementManager _elementManager;
        private readonly ILayoutManager _layoutManager;
        private readonly Lazy<IContentPartDisplay> _contentPartDisplay;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ILayoutModelMapper _mapper;

        public LayoutPartDriver(
            ILayoutSerializer serializer, 
            IElementDisplay elementDisplay, 
            IElementManager elementManager, 
            ILayoutManager layoutManager,
            Lazy<IContentPartDisplay> contentPartDisplay, 
            IShapeDisplay shapeDisplay, 
            ILayoutModelMapper mapper) {

            _serializer = serializer;
            _elementDisplay = elementDisplay;
            _elementManager = elementManager;
            _layoutManager = layoutManager;
            _contentPartDisplay = contentPartDisplay;
            _shapeDisplay = shapeDisplay;
            _mapper = mapper;
        }

        protected override DriverResult Display(LayoutPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Layout", () => {
                    var elements = _layoutManager.LoadElements(part);
                    var layoutRoot = _elementDisplay.DisplayElements(elements, part, displayType: displayType);
                    return shapeHelper.Parts_Layout(LayoutRoot: layoutRoot);
                }),
                ContentShape("Parts_Layout_Summary", () => {
                    var layoutShape = _contentPartDisplay.Value.BuildDisplay(part);
                    var layoutHtml = _shapeDisplay.Display(layoutShape);
                    return shapeHelper.Parts_Layout_Summary(LayoutHtml: layoutHtml);
                }));
        }

        protected override DriverResult Editor(LayoutPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(LayoutPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_Layout_Edit", () => {
                var viewModel = new LayoutPartViewModel {
                    Data = _mapper.ToEditorModel(part.LayoutData, new DescribeElementsContext { Content = part}).ToJson(),
                    ConfigurationData = GetConfigurationData(part),
                    TemplateId = part.TemplateId,
                    Content = part,
                    SessionKey = part.SessionKey,
                    Templates = _layoutManager.GetTemplates().Where(x => x.Id != part.Id).ToArray()
                };

                if (updater != null) {
                    updater.TryUpdateModel(viewModel, Prefix, null, new[] { "Part", "Templates" });
                    var describeContext = new DescribeElementsContext { Content = part };
                    var elementInstances = _mapper.ToLayoutModel(viewModel.Data, describeContext).ToArray();
                    var removedElementInstances = _serializer.Deserialize(viewModel.Trash, describeContext).ToArray();
                    var context = new LayoutSavingContext {
                        Content = part,
                        Updater = updater,
                        Elements = elementInstances,
                        RemovedElements = removedElementInstances
                    };
                    
                    _elementManager.Saving(context);
                    _elementManager.Removing(context);

                    part.LayoutData = _serializer.Serialize(elementInstances);
                    part.TemplateId = viewModel.TemplateId;
                    part.SessionKey = viewModel.SessionKey;
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.Layout", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override void Exporting(LayoutPart part, ExportContentContext context) {
            _layoutManager.Exporting(new ExportLayoutContext { Layout = part });

            context.Element(part.PartDefinition.Name).SetElementValue("LayoutData", part.LayoutData);

            if (part.TemplateId != null) {
                var template = part.ContentItem.ContentManager.Get(part.TemplateId.Value);

                if (template != null) {
                    var templateIdentity = part.ContentItem.ContentManager.GetItemMetadata(template).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("TemplateId", templateIdentity);
                }
            }
        }

        protected override void Importing(LayoutPart part, ImportContentContext context) {
            context.ImportChildEl(part.PartDefinition.Name, "LayoutData", s => {
                part.LayoutData = s;
                _layoutManager.Importing(new ImportLayoutContext {
                    Layout = part,
                    Session = new ImportContentContextWrapper(context)
                });
            });
         
            context.ImportAttribute(part.PartDefinition.Name, "TemplateId", s => part.TemplateId = GetTemplateId(context, s));
        }

        private static int? GetTemplateId(ImportContentContext context, string templateIdentity) {
            if (String.IsNullOrWhiteSpace(templateIdentity))
                return null;

            var template = context.GetItemFromSession(templateIdentity);
            return template != null ? template.Id : default(int?);
        }

        private string GetConfigurationData(LayoutPart part) {
            var elementCategories = GetCategories(part).ToArray();
            var config = new {
                categories = elementCategories.Select(category => new {
                    name = category.DisplayName.Text,
                    contentTypes = category.Elements.Where(x => !x.IsSystemElement).Select(descriptor => {
                        var element = _elementManager.ActivateElement(descriptor);
                        var map = _mapper.GetMapFor(element);
                        return new {
                            label = descriptor.DisplayText.Text,
                            id = descriptor.TypeName,
                            type = map.LayoutElementType,
                            typeClass = descriptor.DisplayText.Text.HtmlClassify(),
                            description = descriptor.Description.Text,
                            icon = descriptor.ToolboxIcon,
                            hasEditor = descriptor.EnableEditorDialog,

                            // If the element has no editor then the toolbox will add the element straight to to designer when being dragged & dropped,
                            // so we'll want to present the user with a prerendered element.
                            html = descriptor.EnableEditorDialog ? "" : RenderElement(element, new DescribeElementsContext { Content = part})
                        };
                    })
                })
            };

            return JToken.FromObject(config).ToString(Formatting.None);
        }

        private string RenderElement(Element element, DescribeElementsContext describeContext, string displayType = "Design") {
            return _shapeDisplay.Display(_elementDisplay.DisplayElement(element, describeContext.Content, displayType));
        }

        private IEnumerable<CategoryDescriptor> GetCategories(LayoutPart part) {
            var describeContext = new DescribeElementsContext { Content = part };
            var elementCategories = _elementManager.GetCategories(describeContext).ToArray();

            return elementCategories.Where(category => category.Elements.Any(x => !x.IsSystemElement));
        }
    }
}