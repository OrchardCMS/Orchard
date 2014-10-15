using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;
using Orchard.UI.Zones;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Framework.Display {
    public class ElementDisplay : IElementDisplay {
        private readonly IShapeFactory _shapeFactory;
        private readonly IElementEventHandler _elementEventHandlerHandler;

        public ElementDisplay(IShapeFactory shapeFactory, IElementEventHandler elementEventHandlerHandler) {
            _shapeFactory = shapeFactory;
            _elementEventHandlerHandler = elementEventHandlerHandler;
        }

        public dynamic DisplayElement(
            IElement element, 
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

            var elementShapeArguments = CreateArguments(element, content, element.State);
            var elementShape = (dynamic)_shapeFactory.Create("Element", elementShapeArguments, () => new ZoneHolding(() => _shapeFactory.Create("ElementZone")));
            var typeName = element.GetType().Name;
            var category = element.Category.ToSafeName();
            elementShape.Metadata.DisplayType = displayType;
            elementShape.Metadata.Alternates.Add(String.Format("Element_{0}", displayType));
            elementShape.Metadata.Alternates.Add(String.Format("Element__{0}", typeName));
            elementShape.Metadata.Alternates.Add(String.Format("Element__{0}__{1}", category, typeName));
            elementShape.Metadata.Alternates.Add(String.Format("Element_{0}__{1}", displayType, typeName));
            elementShape.Metadata.Alternates.Add(String.Format("Element_{0}__{1}__{2}", displayType, category, typeName));

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
            element.Descriptor.Displaying(displayContext);

            var container = element as IContainer;

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

        public dynamic DisplayElements(IEnumerable<IElement> elements, IContent content, string displayType = null, IUpdateModel updater = null, string renderEventName = null, string renderEventArgs = null) {
            var layoutRoot = (dynamic) _shapeFactory.Create("LayoutRoot");

            foreach (var element in elements) {
                var elementShape = DisplayElement(element, content, displayType, updater, renderEventName, renderEventArgs);
                layoutRoot.Add(elementShape);
            }

            return layoutRoot;
        }

        private static INamedEnumerable<object> CreateArguments(IElement element, IContent content, StateDictionary elementState) {
            var children = new List<dynamic>();
            var dictionary = new Dictionary<string, object> {
                {"Element", element},
                {"Elements", children},
                {"ContentItem", content.ContentItem}
            };

            if (elementState != null) {
                foreach (var entry in elementState) {
                    dictionary[MakeValidName(entry.Key)] = entry.Value;
                }
            }

            return Arguments.From(dictionary);
        }

        private static string MakeValidName(string key) {
            return key.Replace(".", "_");
        }
    }
}