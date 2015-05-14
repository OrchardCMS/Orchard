using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.ViewModels;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Services {
    public class LayoutEditorFactory : ILayoutEditorFactory {
        private readonly ILayoutModelMapper _mapper;
        private readonly ILayoutManager _layoutManager;
        private readonly IElementManager _elementManager;
        private readonly IElementDisplay _elementDisplay;
        private readonly IShapeDisplay _shapeDisplay;

        public LayoutEditorFactory(
            ILayoutModelMapper mapper, 
            ILayoutManager layoutManager, 
            IElementManager elementManager, 
            IElementDisplay elementDisplay, 
            IShapeDisplay shapeDisplay) {

            _mapper = mapper;
            _layoutManager = layoutManager;
            _elementManager = elementManager;
            _elementDisplay = elementDisplay;
            _shapeDisplay = shapeDisplay;
        }

        public LayoutEditor Create(LayoutPart layoutPart) {
            return Create(layoutPart.LayoutData, layoutPart.SessionKey, layoutPart.TemplateId, layoutPart);
        }

        public LayoutEditor Create(string layoutData, string sessionKey, int? templateId = null, IContent content = null) {
            return new LayoutEditor {
                Data = _mapper.ToEditorModel(layoutData, new DescribeElementsContext {Content = content}).ToJson(),
                ConfigurationData = GetConfigurationData(content),
                TemplateId = templateId,
                Content = content,
                SessionKey = sessionKey,
                Templates = GetTemplates(content)
            };
        }

        private IList<LayoutPart> GetTemplates(IContent content = null) {
            var query = _layoutManager.GetTemplates();
            var layoutPart = content != null ? content.As<LayoutPart>() : null;

            if (layoutPart != null) {
                query = query.Where(x => x.Id != layoutPart.Id);
            }

            return query.ToList();
        }

        private string GetConfigurationData(IContent content) {
            var elementCategories = GetCategories(content).ToArray();
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
                            html = descriptor.EnableEditorDialog ? "" : RenderElement(element, new DescribeElementsContext { Content = content })
                        };
                    })
                })
            };

            return JToken.FromObject(config).ToString(Formatting.None);
        }

        private string RenderElement(Element element, DescribeElementsContext describeContext, string displayType = "Design") {
            return _shapeDisplay.Display(_elementDisplay.DisplayElement(element, describeContext.Content, displayType));
        }

        private IEnumerable<CategoryDescriptor> GetCategories(IContent content) {
            var describeContext = new DescribeElementsContext { Content = content };
            var elementCategories = _elementManager.GetCategories(describeContext).ToArray();

            return elementCategories.Where(category => category.Elements.Any(x => !x.IsSystemElement));
        }
    }
}