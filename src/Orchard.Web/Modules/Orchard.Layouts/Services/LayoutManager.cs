using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Settings;
using Orchard.Validation;

namespace Orchard.Layouts.Services {
    public class LayoutManager : ILayoutManager {
        private readonly IContentManager _contentManager;
        private readonly ILayoutSerializer _serializer;
        private readonly IElementDisplay _elementDisplay;
        private readonly IElementManager _elementManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public LayoutManager(
            IContentManager contentManager, 
            ILayoutSerializer serializer, 
            IElementDisplay elementDisplay, 
            IElementManager elementManager, 
            ICacheManager cacheManager, 
            ISignals signals) {

            _contentManager = contentManager;
            _serializer = serializer;
            _elementDisplay = elementDisplay;
            _elementManager = elementManager;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public IEnumerable<LayoutPart> GetTemplates() {
            var templateTypeNamesQuery = from typeDefinition in _contentManager.GetContentTypeDefinitions()
                                         from typePartDefinition in typeDefinition.Parts
                                         let settings = typePartDefinition.Settings.GetModel<LayoutTypePartSettings>()
                                         where settings.IsTemplate
                                         select typeDefinition.Name;

            var templateTypeNames = templateTypeNamesQuery.ToArray();
            return _contentManager.Query<LayoutPart>(templateTypeNames).List();
        }

        public IEnumerable<LayoutPart> GetLayouts() {
            var templateTypeNamesQuery = from typeDefinition in _contentManager.GetContentTypeDefinitions()
                                         from typePartDefinition in typeDefinition.Parts
                                         where typePartDefinition.PartDefinition.Name == "LayoutPart"
                                         select typeDefinition.Name;

            var templateTypeNames = templateTypeNamesQuery.ToArray();
            return _contentManager.Query<LayoutPart>(templateTypeNames).List();
        }

        public LayoutPart GetLayout(int id) {
            return _contentManager.Get<LayoutPart>(id);
        }

        public IEnumerable<Element> LoadElements(ILayoutAspect layout) {
            var describeContext = new DescribeElementsContext { Content = layout };
            return _serializer.Deserialize(layout.LayoutData, describeContext);
        }

        public void Exporting(ExportLayoutContext context) {
            var elementTree = LoadElements(context.Layout).ToArray();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Exporting(elements, context);
            context.Layout.LayoutData = _serializer.Serialize(elementTree);
        }

        public void Importing(ImportLayoutContext context) {
            var elementTree = LoadElements(context.Layout).ToArray();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Importing(elements, context);
            context.Layout.LayoutData = _serializer.Serialize(elementTree);
        }

        public IEnumerable<string> GetZones() {
            return _cacheManager.Get("LayoutZones", context => {
                context.Monitor(_signals.When(Signals.LayoutZones));
                return GetZones(GetLayouts());
            });
        }

        public IEnumerable<string> GetZones(ILayoutAspect layout) {
            Argument.ThrowIfNull(layout, "layout");

            var key = String.Format("LayoutZones-{0}", layout.Id);
            return _cacheManager.Get(key, context => {
                context.Monitor(_signals.When(Signals.LayoutZones));

                var layouts = new List<ILayoutAspect>();
                var currentTemplate = layout.TemplateId != null ? GetLayout(layout.TemplateId.Value) : default(LayoutPart);

                // Add the layout itself to the chain of layouts to harvest zones from.
                layouts.Add(layout);

                // Walk up the chain of templates and collect each one for zone harvesting.
                while (currentTemplate != null) {
                    layouts.Add(currentTemplate);
                    currentTemplate = currentTemplate.TemplateId != null ? GetLayout(currentTemplate.TemplateId.Value) : default(LayoutPart);
                }

                // Harvest the zones from the chain of layouts.
                return GetZones(layouts);
            });
        }

        public dynamic RenderLayout(string data, string displayType = null, IContent content = null) {
            var elements = _serializer.Deserialize(data, new DescribeElementsContext { Content = content });
            var layoutRoot = _elementDisplay.DisplayElements(elements, content, displayType);
            return layoutRoot;
        }

        public IEnumerable<Element> ApplyTemplate(LayoutPart layout) {
            var templateLayoutPart = layout.TemplateId != null ? GetLayout(layout.TemplateId.Value) : default(LayoutPart);

            // Update the layout with the selected template, if any.
            return templateLayoutPart != null ? ApplyTemplate(layout, templateLayoutPart) : null;
        }

        public IEnumerable<Element> ApplyTemplate(LayoutPart layout, LayoutPart templateLayout) {
            return ApplyTemplate(LoadElements(layout), LoadElements(templateLayout));
        }

        public IEnumerable<Element> ApplyTemplate(IEnumerable<Element> layout, IEnumerable<Element> templateLayout) {
            var template = Templatify(templateLayout).ToList();
            var templateColumns = ExtractColumns(template).ToArray();
            var layoutColumns = ExtractColumns(layout).ToArray();
            var nonTemplatedElements = ExtractNonTemplatedElements(layout).ToList();

            foreach (var element in nonTemplatedElements) {
                
                // Move the element to the template and try to maintain its index.
                var column = element.Container as Column;
                var indexInTemplate = templateColumns.Any() ? 0 : -1;
                if (column != null) {
                    var indexInLayout = Array.IndexOf(layoutColumns, column);
                    indexInTemplate = indexInLayout < templateColumns.Count() ? indexInLayout : templateColumns.Any() ? 0 : -1;
                }

                if (indexInTemplate > -1) {
                    templateColumns[indexInTemplate].Elements.Add(element);
                }
                else {
                    template.Add(element);
                }
            }

            return template;
        }

        public IEnumerable<Element> DetachTemplate(IEnumerable<Element> elements) {
            var canvas = (Canvas)elements.FirstOrDefault(x => x is Canvas) ?? _elementManager.ActivateElement<Canvas>();
            var nonTemplatedElements = ExtractNonTemplatedElements(elements).ToList();

            canvas.IsTemplated = false;
            canvas.Elements = nonTemplatedElements;

            yield return canvas;
        }

        public IEnumerable<LayoutPart> GetTemplateClients(int templateId, VersionOptions versionOptions) {
            return _contentManager.Query<LayoutPart, LayoutPartRecord>(versionOptions).Where(x => x.TemplateId == templateId).List().ToArray();
        }

        public IEnumerable<Element> CreateDefaultLayout() {
            var canvas = _elementManager.ActivateElement<Canvas>();
            var grid = _elementManager.ActivateElement<Grid>();
            var row = _elementManager.ActivateElement<Row>();
            var column = _elementManager.ActivateElement<Column>();

            canvas.Elements.Add(grid);
            grid.Elements.Add(row);
            row.Elements.Add(column);

            var elements = new List<Element> { canvas };
            return elements;
        }

        private static IEnumerable<Element> Templatify(IEnumerable<Element> elements, bool templatify = true) {
            foreach (var element in elements.Flatten()) {
                element.IsTemplated = templatify;
            }
            return elements;
        }

        private IEnumerable<Column> ExtractColumns(IEnumerable<Element> elements) {
            return elements.Flatten().Where(x => x is Column).Cast<Column>();
        }

        private IEnumerable<Element> ExtractNonTemplatedElements(IEnumerable<Element> elements) {
            foreach (var element in elements) {
                if (!element.IsTemplated && !(element is Canvas)) {
                    yield return element;
                }
                else {
                    var container = element as Container;

                    if (container != null) {
                        foreach (var child in ExtractNonTemplatedElements(container.Elements)) {
                            yield return child;
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetZones(IEnumerable<ILayoutAspect> layouts) {
            var zoneNames = new HashSet<string>();

            foreach (var layoutPart in layouts) {
                var elements = LoadElements(layoutPart).Flatten();
                var columns = elements.Where(x => x is Column).Cast<Column>().Where(x => !String.IsNullOrWhiteSpace(x.ZoneName)).ToList();

                foreach (var column in columns)
                    zoneNames.Add(column.ZoneName);
            }

            return zoneNames.OrderBy(x => x);
        }
    }
}