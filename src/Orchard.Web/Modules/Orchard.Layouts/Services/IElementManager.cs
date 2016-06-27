using System;
using System.Collections.Generic;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public interface IElementManager : IDependency {
        IEnumerable<ElementDescriptor> DescribeElements(DescribeElementsContext context);
        IEnumerable<CategoryDescriptor> GetCategories(DescribeElementsContext context);
        ElementDescriptor GetElementDescriptorByTypeName(DescribeElementsContext context, string typeName);
        ElementDescriptor GetElementDescriptorByType<T>(DescribeElementsContext context) where T : Element;
        ElementDescriptor GetElementDescriptorByType<T>() where T : Element;
        Element ActivateElement(ElementDescriptor descriptor, Action<Element> initialize = null);
        T ActivateElement<T>(ElementDescriptor descriptor, Action<T> initialize = null) where T : Element;
        T ActivateElement<T>(Action<T> initialize = null) where T : Element;
        IEnumerable<IElementDriver> GetDrivers<TElement>() where TElement : Element;
        IEnumerable<IElementDriver> GetDrivers(ElementDescriptor descriptor);
        IEnumerable<IElementDriver> GetDrivers(Element element);
        IEnumerable<IElementDriver> GetDrivers(Type elementType);
        IEnumerable<IElementDriver> GetDrivers();
        EditorResult BuildEditor(ElementEditorContext context);
        EditorResult UpdateEditor(ElementEditorContext context);
        void Saving(LayoutSavingContext context);
        void Removing(LayoutSavingContext context);
        void Exporting(IEnumerable<Element> elements, ExportLayoutContext context);
        void Exported(IEnumerable<Element> elements, ExportLayoutContext context);
        void Importing(IEnumerable<Element> elements, ImportLayoutContext context);
        void Imported(IEnumerable<Element> elements, ImportLayoutContext context);
        void ImportCompleted(IEnumerable<Element> elements, ImportLayoutContext context);
    }
}