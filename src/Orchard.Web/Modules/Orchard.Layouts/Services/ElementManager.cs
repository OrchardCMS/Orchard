using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Services {
    public class ElementManager : Component, IElementManager {
        private readonly Lazy<IEnumerable<ElementHarvester>> _elementHarvesters;
        private readonly ICacheManager _cacheManager;
        private readonly Lazy<IEnumerable<IElementDriver>> _drivers;
        private readonly Lazy<IEnumerable<ICategoryProvider>> _categoryProviders;
        private readonly IElementFactory _factory;
        private readonly ISignals _signals;
        private readonly IElementEventHandler _elementEventHandler;

        public ElementManager(
            Lazy<IEnumerable<ElementHarvester>> elementHarvesters,
            ICacheManager cacheManager,
            Lazy<IEnumerable<IElementDriver>> drivers,
            Lazy<IEnumerable<ICategoryProvider>> categoryProviders,
            IElementFactory factory,
            ISignals signals,
            IElementEventHandler elementEventHandler) {

            _elementHarvesters = elementHarvesters;
            _cacheManager = cacheManager;
            _drivers = drivers;
            _categoryProviders = categoryProviders;
            _factory = factory;
            _signals = signals;
            _elementEventHandler = elementEventHandler;
        }

        public IEnumerable<ElementDescriptor> DescribeElements(DescribeElementsContext context) {
            var contentType = context.Content != null ? context.Content.ContentItem.ContentType : default(string);
            var cacheKey = String.Format("LayoutElementTypes-{0}-{1}", contentType ?? "AnyType", context.CacheVaryParam);
            return _cacheManager.Get(cacheKey, true, acquireContext => {
                var harvesterContext = new HarvestElementsContext {
                    Content = context.Content
                };
                var query =
                    from harvester in _elementHarvesters.Value
                    from elementDescriptor in harvester.HarvestElements(harvesterContext)
                    orderby elementDescriptor.DisplayText.Text
                    select elementDescriptor;

                acquireContext.Monitor(_signals.When(Signals.ElementDescriptors));
                return query.ToArray();
            });
        }

        public IEnumerable<CategoryDescriptor> GetCategories(DescribeElementsContext context) {
            var contentType = context.Content != null ? context.Content.ContentItem.ContentType : default(string);
            return _cacheManager.Get(String.Format("ElementCategories-{0}-{1}", contentType ?? "AnyType", context.CacheVaryParam), true, acquireContext => {
                var elements = DescribeElements(context);
                var categoryDictionary = GetCategories();
                var categoryDescriptorDictionary = new Dictionary<string, CategoryDescriptor>();

                foreach (var element in elements) {
                    var category = categoryDictionary.ContainsKey(element.Category)
                        ? categoryDictionary[element.Category]
                        : new Category(element.Category, T(element.Category), position: int.MaxValue);

                    var descriptor = categoryDescriptorDictionary.ContainsKey(element.Category)
                        ? categoryDescriptorDictionary[element.Category]
                        : categoryDescriptorDictionary[element.Category] = new CategoryDescriptor(category.Name, category.DisplayName, category.Description, category.Position);

                    descriptor.Elements.Add(element);
                }

                acquireContext.Monitor(_signals.When(Signals.ElementDescriptors));
                return categoryDescriptorDictionary.Values.OrderBy(x => x.Position);
            });
        }

        public ElementDescriptor GetElementDescriptorByTypeName(DescribeElementsContext context, string typeName) {
            var elements = DescribeElements(context);
            var element = elements.SingleOrDefault(x => x.TypeName == typeName);

            return element;
        }

        public ElementDescriptor GetElementDescriptorByType<T>(DescribeElementsContext context) where T : Element {
            return GetElementDescriptorByTypeName(context, typeof(T).FullName);
        }

        public ElementDescriptor GetElementDescriptorByType<T>() where T : Element {
            return GetElementDescriptorByTypeName(DescribeElementsContext.Empty, typeof(T).FullName);
        }

        public Element ActivateElement(ElementDescriptor descriptor, Action<Element> initialize = null) {
            return _factory.Activate(descriptor, initialize);
        }

        public T ActivateElement<T>(ElementDescriptor descriptor, Action<T> initialize = null) where T : Element {
            return _factory.Activate(descriptor, initialize);
        }

        public T ActivateElement<T>(Action<T> initialize = null) where T : Element {
            var context = DescribeElementsContext.Empty;
            var descriptor = GetElementDescriptorByType<T>(context);
            return ActivateElement<T>(descriptor, initialize);
        }

        public IEnumerable<IElementDriver> GetDrivers<TElement>() where TElement : Element {
            return GetDrivers(typeof(TElement));
        }

        public IEnumerable<IElementDriver> GetDrivers(ElementDescriptor descriptor) {
            return descriptor.GetDrivers();
        }

        public IEnumerable<IElementDriver> GetDrivers(Element element) {
            return GetDrivers(element.GetType());
        }

        public IEnumerable<IElementDriver> GetDrivers(Type elementType) {
            var drivers = _drivers.Value.Where(x => IsElementType(x, elementType)).OrderByDescending(x => x.Priority).ToArray();
            return drivers;
        }

        public IEnumerable<IElementDriver> GetDrivers() {
            return _drivers.Value.OrderByDescending(x => x.Priority).ToArray();
        }

        public EditorResult BuildEditor(ElementEditorContext context) {
            _elementEventHandler.BuildEditor(context);
            context.Element.Descriptor.Editor(context);
            return context.EditorResult;
        }

        public EditorResult UpdateEditor(ElementEditorContext context) {
            _elementEventHandler.UpdateEditor(context);
            context.Element.Descriptor.UpdateEditor(context);
            return context.EditorResult;
        }

        public void Saving(LayoutSavingContext context) {
            var elements = context.Elements.Flatten();
            InvokeDriver(elements, (driver, element) => driver.LayoutSaving(new ElementSavingContext(context) {
                Element = element
            }));
        }

        public void Removing(LayoutSavingContext context) {
            var elementInstances = context.RemovedElements.Flatten();
            InvokeDriver(elementInstances, (driver, elementInstance) => driver.Removing(new ElementRemovingContext(context) {
                Element = elementInstance
            }));
        }

        public void Exporting(IEnumerable<Element> elements, ExportLayoutContext context) {
            InvokeDriver(elements, (driver, element) => {
                var exportElementContext = new ExportElementContext {
                    Layout = context.Layout,
                    Element = element
                };
                driver.Exporting(exportElementContext);
                element.ExportableData = new ElementDataDictionary(exportElementContext.ExportableData);
            });
        }

        public void Importing(IEnumerable<Element> elements, ImportLayoutContext context) {
            InvokeDriver(elements, (driver, element) => {
                var importElementContext = new ImportElementContext {
                    Layout = context.Layout,
                    Element = element,
                    ExportableData = element.ExportableData,
                    Session = context.Session
                };
                driver.Importing(importElementContext);
            });
        }

        private IDictionary<string, Category> GetCategories() {
            var providers = _categoryProviders.Value;
            var categories = providers.SelectMany(x => x.GetCategories());
            var dictionary = new Dictionary<string, Category>();

            foreach (var category in categories) {
                dictionary[category.Name] = category;
            }

            return dictionary;
        }

        private void InvokeDriver(IEnumerable<Element> elements, Action<IElementDriver, Element> driverAction) {
            foreach (var element in elements) {
                var drivers = GetDrivers(element.Descriptor);
                foreach (var driver in drivers) {
                    driverAction(driver, element);
                }
            }
        }

        private static bool IsElementType(IElementDriver elementDriver, Type elementType) {
            var driverType = elementDriver.GetType();
            var driverElementType = driverType.BaseType.GenericTypeArguments[0];
            return driverElementType == elementType || driverElementType.IsAssignableFrom(elementType);
        }
    }
}