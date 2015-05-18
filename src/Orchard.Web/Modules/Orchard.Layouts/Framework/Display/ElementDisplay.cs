using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;
using Orchard.UI.Zones;
using Orchard.Utility.Extensions;
using ContentItem = Orchard.ContentManagement.ContentItem;

namespace Orchard.Layouts.Framework.Display {
    public class ElementDisplay : IElementDisplay {
        private readonly IShapeFactory _shapeFactory;
        private readonly IElementEventHandler _elementEventHandlerHandler;

        public ElementDisplay(IShapeFactory shapeFactory, IElementEventHandler elementEventHandlerHandler) {
            _shapeFactory = shapeFactory;
            _elementEventHandlerHandler = elementEventHandlerHandler;
        }

        public dynamic DisplayElement(
            Element element,
            IContent content,
            string displayType = null,
            IUpdateModel updater = null,
            string renderEventName = null,
            string renderEventArgs = null) {

            var createShapeContext = new ElementCreatingDisplayShapeContext {
                Element = element,
                DisplayType = displayType,
                Content = content,
            };

            element.Descriptor.CreatingDisplay(createShapeContext);

            var typeName = element.GetType().Name;
            var category = element.Category.ToSafeName();
            var drivers = element.Descriptor.GetDrivers();
            var elementShapeArguments = CreateArguments(element, content);
            var elementShape = (dynamic)_shapeFactory.Create("Element", elementShapeArguments, () => new ZoneHolding(() => _shapeFactory.Create("ElementZone")));
            

            elementShape.Metadata.DisplayType = displayType;
            elementShape.Metadata.Alternates.Add(String.Format("Elements_{0}", typeName));
            elementShape.Metadata.Alternates.Add(String.Format("Elements_{0}_{1}", typeName, displayType));
            elementShape.Metadata.Alternates.Add(String.Format("Elements_{0}__{1}", typeName, category));
            elementShape.Metadata.Alternates.Add(String.Format("Elements_{0}_{1}__{2}", typeName, displayType, category));

            var displayContext = new ElementDisplayContext {
                Element = element,
                ElementShape = elementShape,
                DisplayType = displayType,
                Content = content,
                Updater = updater,
                RenderEventName = renderEventName,
                RenderEventArgs = renderEventArgs
            };

            _elementEventHandlerHandler.Displaying(displayContext);
            InvokeDrivers(drivers, driver => driver.Displaying(displayContext));
            element.Descriptor.Display(displayContext);

            var container = element as Container;

            if (container != null) {
                if (container.Elements.Any()) {
                    foreach (var child in container.Elements) {
                        var childShape = DisplayElement(child, content, displayType: displayType, updater: updater);
                        childShape.Parent = elementShape;
                        elementShape.Add(childShape);
                    }
                }
            }

            return elementShape;
        }

        public dynamic DisplayElements(IEnumerable<Element> elements, IContent content, string displayType = null, IUpdateModel updater = null, string renderEventName = null, string renderEventArgs = null) {
            var layoutRoot = (dynamic)_shapeFactory.Create("LayoutRoot");

            foreach (var element in elements) {
                var elementShape = DisplayElement(element, content, displayType, updater, renderEventName, renderEventArgs);
                layoutRoot.Add(elementShape);
            }

            return layoutRoot;
        }

        private static INamedEnumerable<object> CreateArguments(Element element, IContent content) {
            var children = new List<dynamic>();
            var dictionary = new Dictionary<string, object> {
                {"Element", element},
                {"Elements", children},
                {"ContentItem", content != null ? content.ContentItem : default(ContentItem)}
            };

            return Arguments.From(dictionary);
        }

        private void InvokeDrivers(IEnumerable<IElementDriver> drivers, Action<IElementDriver> driverAction) {
            foreach (var driver in drivers) {
                driverAction(driver);
            }
        }
    }
}