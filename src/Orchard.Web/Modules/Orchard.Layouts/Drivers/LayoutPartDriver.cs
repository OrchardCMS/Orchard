using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Layouts.Settings;
using Orchard.Layouts.ViewModels;
using Orchard.Logging;

namespace Orchard.Layouts.Drivers {
    public class LayoutPartDriver : ContentPartDriver<LayoutPart> {
        private readonly ILayoutSerializer _serializer;
        private readonly IElementDisplay _elementDisplay;
        private readonly IElementManager _elementManager;
        private readonly ILayoutManager _layoutManager;
        private readonly Lazy<IContentPartDisplay> _contentPartDisplay;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ILayoutModelMapper _mapper;
        private readonly ILayoutEditorFactory _layoutEditorFactory;
        private readonly HashSet<string> _stack;

        public LayoutPartDriver(
            ILayoutSerializer serializer,
            IElementDisplay elementDisplay,
            IElementManager elementManager,
            ILayoutManager layoutManager,
            Lazy<IContentPartDisplay> contentPartDisplay,
            IShapeDisplay shapeDisplay,
            ILayoutModelMapper mapper,
            ILayoutEditorFactory layoutEditorFactory) {

            _serializer = serializer;
            _elementDisplay = elementDisplay;
            _elementManager = elementManager;
            _layoutManager = layoutManager;
            _contentPartDisplay = contentPartDisplay;
            _shapeDisplay = shapeDisplay;
            _mapper = mapper;
            _layoutEditorFactory = layoutEditorFactory;
            _stack = new HashSet<string>();

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        protected override DriverResult Display(LayoutPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Layout", () => {
                    if (DetectRecursion(part, "Parts_Layout"))
                        return shapeHelper.Parts_Layout_Recursive();

                    var elements = _layoutManager.LoadElements(part);
                    var layoutRoot = _elementDisplay.DisplayElements(elements, part, displayType: displayType);
                    return shapeHelper.Parts_Layout(LayoutRoot: layoutRoot);
                }),
                ContentShape("Parts_Layout_Summary", () => {
                    if (DetectRecursion(part, "Parts_Layout_Summary"))
                        return shapeHelper.Parts_Layout_Summary_Recursive();

                    var layoutShape = _contentPartDisplay.Value.BuildDisplay(part);
                    var layoutHtml = _shapeDisplay.Display(layoutShape);
                    return shapeHelper.Parts_Layout_Summary(LayoutHtml: layoutHtml);
                }));
        }

        private bool DetectRecursion(LayoutPart part, string shapeName) {
            var key = String.Format("{0}:{1}", shapeName, part.Id);

            if (_stack.Contains(key)) {
                Logger.Debug(String.Format("Detected recursive layout rendering of layout with ID = {0} and shape = {1}", part.Id, shapeName));
                return true;
            }

            _stack.Add(key);
            return false;
        }

        protected override DriverResult Editor(LayoutPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(LayoutPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_Layout_Edit", () => {
                if (part.Id == 0 && String.IsNullOrWhiteSpace(part.LayoutData)) {

                    var settings = part.TypePartDefinition.Settings.GetModel<LayoutTypePartSettings>();

                    // If the default layout setting is left empty, use the one from the service
                    if (String.IsNullOrWhiteSpace(settings.DefaultLayoutData)) {
                        var defaultData = _serializer.Serialize(_layoutManager.CreateDefaultLayout());
                        part.LayoutData = defaultData;
                    }
                    else {
                        part.LayoutData = settings.DefaultLayoutData;
                    }
                }

                var viewModel = new LayoutPartViewModel {
                    LayoutEditor = _layoutEditorFactory.Create(part)
                };

                if (updater != null) {
                    updater.TryUpdateModel(viewModel, Prefix, null, new[] { "Part", "Templates" });
                    var describeContext = new DescribeElementsContext { Content = part };
                    var elementInstances = _mapper.ToLayoutModel(viewModel.LayoutEditor.Data, describeContext).ToArray();
                    var recycleBin = (RecycleBin)_mapper.ToLayoutModel(viewModel.LayoutEditor.RecycleBin, describeContext).SingleOrDefault();
                    var context = new LayoutSavingContext {
                        Content = part,
                        Updater = updater,
                        Elements = elementInstances,
                        RemovedElements = recycleBin != null ? recycleBin.Elements : Enumerable.Empty<Element>()
                    };

                    _elementManager.Saving(context);
                    _elementManager.Removing(context);

                    part.LayoutData = _serializer.Serialize(elementInstances);
                    part.TemplateId = viewModel.LayoutEditor.TemplateId;
                    part.SessionKey = viewModel.LayoutEditor.SessionKey;
                    viewModel.LayoutEditor.Data = _mapper.ToEditorModel(part.LayoutData, new DescribeElementsContext { Content = part }).ToJson();
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.Layout", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override void Exporting(LayoutPart part, ExportContentContext context) {
            _layoutManager.Exporting(new ExportLayoutContext { Layout = part });
            
            if (part.TemplateId != null) {
                var template = part.ContentItem.ContentManager.Get(part.TemplateId.Value);

                if (template != null) {
                    var templateIdentity = part.ContentItem.ContentManager.GetItemMetadata(template).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("TemplateId", templateIdentity);
                }
            }

            context.Element(part.PartDefinition.Name).Add(new XElement("LayoutData", new XCData(WriteFormattedLayoutData(part.LayoutData))));
        }

        protected override void Exported(LayoutPart part, ExportContentContext context)
        {
            _layoutManager.Exported(new ExportLayoutContext { Layout = part });
        }

        protected override void Importing(LayoutPart part, ImportContentContext context) {
            HandleImportEvent(part, context, importLayoutContext =>
            {
                var layoutDataElement = context.Data.Element(part.PartDefinition.Name).Element("LayoutData");

                if (layoutDataElement.FirstNode is XCData)
                    part.LayoutData = ReadFormattedLayoutData(((XCData)layoutDataElement.FirstNode).Value);
                else
                    part.LayoutData = layoutDataElement.Value;

                _layoutManager.Importing(importLayoutContext);

                context.ImportAttribute(part.PartDefinition.Name, "TemplateId", s => part.TemplateId = GetTemplateId(context, s));
            });
        }

        protected override void Imported(LayoutPart part, ImportContentContext context) {
            HandleImportEvent(part, context, importLayoutContext => _layoutManager.Imported(importLayoutContext));
        }

        protected override void ImportCompleted(LayoutPart part, ImportContentContext context) {
            HandleImportEvent(part, context, importLayoutContext => _layoutManager.ImportCompleted(importLayoutContext));
        }

        private void HandleImportEvent(LayoutPart part, ImportContentContext context, Action<ImportLayoutContext> callback) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            callback(new ImportLayoutContext {
                Layout = part,
                Session = new ImportContentContextWrapper(context)
            });
        }

        private static int? GetTemplateId(ImportContentContext context, string templateIdentity) {
            if (String.IsNullOrWhiteSpace(templateIdentity))
                return null;

            var template = context.GetItemFromSession(templateIdentity);
            return template != null ? template.Id : default(int?);
        }

        private string WriteFormattedLayoutData(string layoutDataString)
        {
            var layoutData = JsonConvert.DeserializeObject(layoutDataString);
            var formattedLayoutData = JsonConvert.SerializeObject(layoutData, Formatting.Indented);
            return String.Format("\n{0}\n", formattedLayoutData);
        }

        private string ReadFormattedLayoutData(string formattedLayoutData)
        {
            var layoutData = JsonConvert.DeserializeObject(formattedLayoutData);
            var layoutDataString = JsonConvert.SerializeObject(layoutData, Formatting.None);
            return layoutDataString;
        }

        protected override void Cloning(LayoutPart originalPart, LayoutPart clonePart, CloneContentContext context) {
            clonePart.LayoutData = originalPart.LayoutData;
            clonePart.TemplateId = originalPart.TemplateId;
        }
    }
}