using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Serialization;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Settings;

namespace Orchard.Layouts.Services {
    public class LayoutManager : ILayoutManager {
        private readonly IContentManager _contentManager;
        private readonly ILayoutSerializer _serializer;
        private readonly IElementDisplay _elementDisplay;
        private readonly IElementManager _elementManager;

        public LayoutManager(IContentManager contentManager, ILayoutSerializer serializer, IElementDisplay elementDisplay, IElementManager elementManager) {
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

        public LayoutPart GetLayout(int id) {
            return _contentManager.Get<LayoutPart>(id);
        }

        public IEnumerable<IElement> LoadElements(ILayoutAspect layout) {
            var describeContext = new DescribeElementsContext { Content = layout };
            return _serializer.Deserialize(layout.LayoutState, describeContext);
        }

        public void Exporting(ExportLayoutContext context) {
            var elementTree = LoadElements(context.Layout).ToArray();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Exporting(elements, context);
            context.Layout.LayoutState = _serializer.Serialize(elementTree);
        }

        public void Importing(ImportLayoutContext context) {
            var elementTree = LoadElements(context.Layout).ToArray();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Importing(elements, context);
            context.Layout.LayoutState = _serializer.Serialize(elementTree);
        }

        public dynamic RenderLayout(string state, string displayType = null, IContent content = null) {
            var elements = _serializer.Deserialize(state, new DescribeElementsContext { Content = content });
            var layoutRoot = _elementDisplay.DisplayElements(elements, content, displayType);
            return layoutRoot;
        }

        public string ApplyTemplate(LayoutPart layout) {
            var templateLayoutPart = layout.TemplateId != null ? GetLayout(layout.TemplateId.Value) : default(LayoutPart);

            // Update the layout with the selected template, if any.
            return templateLayoutPart != null ? ApplyTemplate(layout, templateLayoutPart) : null;
        }

        public string ApplyTemplate(LayoutPart layout, LayoutPart templateLayout) {
            return ApplyTemplate(LoadElements(layout), LoadElements(templateLayout));
        }

        public string ApplyTemplate(IEnumerable<IElement> layout, IEnumerable<IElement> templateLayout) {
            var template = Templatify(templateLayout).ToList();
            var templateColumns = ExtractColumns(template).ToArray();
            var layoutColumns = ExtractColumns(layout).ToArray();

            foreach (var element in layout.Flatten(levels: 3)) {
                // Ignore root grids, rows, columns and templated elements, as they are considered to be part of the current layout.
                if ((element is IGrid && element.Container == null) || element is IRow || element is IColumn || element.IsTemplated)
                    continue;

                // Move the element to the template and try to maintain its index.
                var column = element.Container as IColumn;
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

            return _serializer.Serialize(template);
        }

        public string DetachTemplate(IEnumerable<IElement> elements) {
            Templatify(elements, false);
            return _serializer.Serialize(elements);
        }

        public IEnumerable<LayoutPart> GetTemplateClients(int templateId, VersionOptions versionOptions) {
            return _contentManager.Query<LayoutPart, LayoutPartRecord>(versionOptions).Where(x => x.TemplateId == templateId).List().ToArray();
        }

        public IEnumerable<IElement> CreateDefaultLayout() {
            var grid = _elementManager.ActivateElement<Grid>();
            var row = _elementManager.ActivateElement<Row>();
            var column = _elementManager.ActivateElement<Column>();

            grid.Elements.Add(row);
            row.Elements.Add(column);

            var elements = new List<IElement> { grid };
            return elements;
        }

        private static IEnumerable<IElement> Templatify(IEnumerable<IElement> elements, bool templatify = true) {
            foreach (var element in elements.Flatten()) {
                element.IsTemplated = templatify;
            }
            return elements;
        }

        private IEnumerable<IColumn> ExtractColumns(IEnumerable<IElement> elements) {
            return elements.Flatten(levels: 3).Where(x => x is IColumn).Cast<IColumn>();
        }
    }
}