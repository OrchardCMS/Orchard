using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Settings;

namespace Orchard.Layouts.Services {
    public class LayoutManager : ILayoutManager {
        private readonly IContentManager _contentManager;
        private readonly ILayoutSerializer _serializer;
        private readonly IElementDisplay _elementDisplay;
        private readonly IElementManager _elementManager;

        public LayoutManager(
            IContentManager contentManager,
            ILayoutSerializer serializer,
            IElementDisplay elementDisplay,
            IElementManager elementManager) {

            _contentManager = contentManager;
            _serializer = serializer;
            _elementDisplay = elementDisplay;
            _elementManager = elementManager;
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

        public void Exported(ExportLayoutContext context) {
            var elementTree = LoadElements(context.Layout).ToArray();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Exported(elements, context);
            context.Layout.LayoutData = _serializer.Serialize(elementTree);
        }

        public void Importing(ImportLayoutContext context) {
            var elementTree = LoadElements(context.Layout).ToArray();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Importing(elements, context);
            context.Layout.LayoutData = _serializer.Serialize(elementTree);
        }

        public void Imported(ImportLayoutContext context) {
            var elementTree = LoadElements(context.Layout).ToArray();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Imported(elements, context);
            context.Layout.LayoutData = _serializer.Serialize(elementTree);
        }

        public void ImportCompleted(ImportLayoutContext context) {
            var elementTree = LoadElements(context.Layout).ToArray();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.ImportCompleted(elements, context);
            context.Layout.LayoutData = _serializer.Serialize(elementTree);
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
    }
}