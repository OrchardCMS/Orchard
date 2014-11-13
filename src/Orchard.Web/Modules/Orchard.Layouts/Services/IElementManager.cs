using System;
using System.Collections.Generic;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public interface IElementManager : IDependency {
        IEnumerable<ElementDescriptor> DescribeElements(DescribeElementsContext context);
        IEnumerable<CategoryDescriptor> GetCategories(DescribeElementsContext context);
        ElementDescriptor GetElementDescriptorByTypeName(DescribeElementsContext context, string typeName);
        ElementDescriptor GetElementDescriptorByType<T>(DescribeElementsContext context) where T : IElement;
        ElementDescriptor GetElementDescriptorByType<T>() where T : IElement;
        IElement ActivateElement(ElementDescriptor descriptor, ActivateElementArgs args = null);
        T ActivateElement<T>(ElementDescriptor descriptor, ActivateElementArgs args = null) where T : IElement;
        T ActivateElement<T>() where T : IElement;
        IEnumerable<IElementDriver> GetDrivers<TElement>() where TElement : IElement;
        IEnumerable<IElementDriver> GetDrivers(ElementDescriptor descriptor);
        IEnumerable<IElementDriver> GetDrivers(IElement element);
        IEnumerable<IElementDriver> GetDrivers(Type elementType);
        IEnumerable<IElementDriver> GetDrivers();
        EditorResult BuildEditor(ElementEditorContext context);
        EditorResult UpdateEditor(ElementEditorContext context);
        void Saving(LayoutSavingContext context);
        void Removing(LayoutSavingContext context);
        void Indexing(LayoutIndexingContext context);
    }
}