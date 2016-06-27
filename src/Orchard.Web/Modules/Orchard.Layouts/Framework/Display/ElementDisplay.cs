using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;
using Orchard.UI.Zones;
using Orchard.Utility.Extensions;
using ContentItem = Orchard.ContentManagement.ContentItem;

namespace Orchard.Layouts.Framework.Display {
    public class ElementDisplay : Component, IElementDisplay {
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
            IUpdateModel updater = null) {

            var typeName = element.GetType().Name;
            var category = element.Category.ToSafeName();
            var drivers = element.Descriptor.GetDrivers().ToList();
            var createShapeContext = new ElementCreatingDisplayShapeContext {
                Element = element,
                DisplayType = displayType,
                Content = content,
            };

            _elementEventHandlerHandler.CreatingDisplay(createShapeContext);
            drivers.Invoke(driver => driver.CreatingDisplay(createShapeContext), Logger);
            if (element.Descriptor.CreatingDisplay != null)
                element.Descriptor.CreatingDisplay(createShapeContext);

            if (createShapeContext.Cancel)
                return null;

            var elementShapeArguments = CreateArguments(element, content);
            var elementShape = (dynamic)_shapeFactory.Create("Element", elementShapeArguments, () => new ZoneHolding(() => _shapeFactory.Create("ElementZone")));

            elementShape.Metadata.DisplayType = displayType;
            elementShape.Metadata.Alternates.Add(String.Format("Elements_{0}", typeName));
            elementShape.Metadata.Alternates.Add(String.Format("Elements_{0}_{1}", typeName, displayType));
            elementShape.Metadata.Alternates.Add(String.Format("Elements_{0}__{1}", typeName, category));
            elementShape.Metadata.Alternates.Add(String.Format("Elements_{0}_{1}__{2}", typeName, displayType, category));

            var displayingContext = new ElementDisplayingContext {
                Element = element,
                ElementShape = elementShape,
                DisplayType = displayType,
                Content = content,
                Updater = updater
            };

            _elementEventHandlerHandler.Displaying(displayingContext);
            drivers.Invoke(driver => driver.Displaying(displayingContext), Logger);

            if (element.Descriptor.Displaying != null)
                element.Descriptor.Displaying(displayingContext);

            var container = element as Container;

            if (container != null) {
                if (container.Elements.Any()) {
                    var childIndex = 0;
                    foreach (var child in container.Elements) {
                        var childShape = DisplayElement(child, content, displayType: displayType, updater: updater);

                        if (childShape != null) {
                            childShape.Parent = elementShape;
                            elementShape.Add(childShape, childIndex++.ToString());
                        }
                    }
                }
            }

            var displayedContext = new ElementDisplayedContext {
                Element = element,
                ElementShape = elementShape,
                DisplayType = displayType,
                Content = content,
                Updater = updater
            };

            _elementEventHandlerHandler.Displayed(displayedContext);
            drivers.Invoke(driver => driver.Displayed(displayedContext), Logger);

            if (element.Descriptor.Displayed != null)
                element.Descriptor.Displayed(displayedContext);

            return elementShape;
        }

        public dynamic DisplayElements(IEnumerable<Element> elements, IContent content, string displayType = null, IUpdateModel updater = null) {
            var layoutRoot = (dynamic)_shapeFactory.Create("LayoutRoot");
            var index = 0;

            foreach (var element in elements) {
                var elementShape = DisplayElement(element, content, displayType, updater);
                layoutRoot.Add(elementShape, index++.ToString());
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
    }
}