using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Harvesters;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Serialization;
using Orchard.Layouts.Services;
using Orchard.Layouts.Shapes;
using Orchard.Layouts.ViewModels;
using Orchard.Localization;
using Orchard.Themes.Services;
using Orchard.Tokens;
using Orchard.Utility.Extensions;
using YamlDotNet.Serialization;

namespace Orchard.Layouts.Providers {
    [OrchardFeature("Orchard.Layouts.Snippets")]
    public class SnippetElementHarvester : IElementHarvester {
        private const string SnippetShapeSuffix = "Snippet";
        private readonly Work<IShapeFactory> _shapeFactory;
        private readonly Work<ISiteThemeService> _siteThemeService;
        private readonly Work<IShapeTableLocator> _shapeTableLocator;
        private readonly Work<IElementFactory> _elementFactory;
        private readonly Work<IShapeDisplay> _shapeDisplay;
        private readonly Work<ICurrentThemeShapeBindingResolver> _currentThemeShapeBindingResolver;
        private readonly Work<ITokenizer> _tokenizer;
        private readonly IWorkContextAccessor _wca;

        public Localizer T;


        public SnippetElementHarvester(
            IWorkContextAccessor workContextAccessor,
            Work<IShapeFactory> shapeFactory,
            Work<ISiteThemeService> siteThemeService,
            Work<IShapeTableLocator> shapeTableLocator,
            Work<IElementFactory> elementFactory,
            Work<IShapeDisplay> shapeDisplay,
            Work<ITokenizer> tokenizer,
            Work<ICurrentThemeShapeBindingResolver> currentThemeShapeBindingResolver) {

            _shapeFactory = shapeFactory;
            _siteThemeService = siteThemeService;
            _shapeTableLocator = shapeTableLocator;
            _elementFactory = elementFactory;
            _shapeDisplay = shapeDisplay;
            _tokenizer = tokenizer;
            _currentThemeShapeBindingResolver = currentThemeShapeBindingResolver;
            _wca = workContextAccessor;

            T = NullLocalizer.Instance;
        }


        public IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context) {
            var currentThemeName = _siteThemeService.Value.GetCurrentThemeName();
            var shapeTable = _shapeTableLocator.Value.Lookup(currentThemeName);
            var shapeDescriptors = shapeTable.Bindings
                .Where(x => !string.Equals(x.Key, "Elements_Snippet", StringComparison.OrdinalIgnoreCase) &&
                    x.Key.EndsWith(SnippetShapeSuffix, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.Key, x => x.Value.ShapeDescriptor);
            var elementType = typeof(Snippet);
            var snippetElement = (Snippet)_elementFactory.Value.Activate(elementType);

            foreach (var shapeDescriptor in shapeDescriptors) {
                var snippetDescriptor = CreateSnippetDescriptor(shapeDescriptor.Value, snippetElement);
                var shapeType = shapeDescriptor.Value.ShapeType;

                yield return new ElementDescriptor(elementType, shapeType, snippetDescriptor.DisplayName, snippetDescriptor.Description, snippetDescriptor.Category) {
                    Displaying = displayContext => Displaying(displayContext, shapeDescriptor.Value, snippetDescriptor),
                    ToolboxIcon = snippetDescriptor.ToolboxIcon,
                    EnableEditorDialog = snippetDescriptor.Fields.Any(),
                    Editor = ctx => Editor(snippetDescriptor, ctx),
                    UpdateEditor = ctx => UpdateEditor(snippetDescriptor, ctx)
                };
            }
        }

        private void Editor(SnippetDescriptor descriptor, ElementEditorContext context) {
            UpdateEditor(descriptor, context);
        }

        private void UpdateEditor(SnippetDescriptor descriptor, ElementEditorContext context) {
            var viewModel = new SnippetViewModel {
                Descriptor = descriptor
            };

            if (context.Updater != null) {
                foreach (var fieldDescriptor in descriptor.Fields) {
                    var name = fieldDescriptor.Name;
                    var result = context.ValueProvider.GetValue(name);

                    if (result == null)
                        continue;

                    context.Element.Data[name] = result.AttemptedValue;
                }
            }

            viewModel.FieldEditors = descriptor.Fields.Select(x => {
                var fieldEditorTemplateName = $"Elements.Snippet.Field.{x.Type ?? "Text"}";
                var fieldDescriptorViewModel = new SnippetFieldViewModel {
                    Descriptor = x,
                    Value = context.Element.Data.Get(x.Name)
                };
                var fieldEditor = context.ShapeFactory.EditorTemplate(TemplateName: fieldEditorTemplateName, Model: fieldDescriptorViewModel, Prefix: context.Prefix);

                return fieldEditor;
            }).ToList();

            var snippetEditorShape = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Snippet", Model: viewModel, Prefix: context.Prefix);
            snippetEditorShape.Metadata.Position = "Fields:0";

            context.EditorResult.Add(snippetEditorShape);
        }

        private void Displaying(ElementDisplayingContext context, ShapeDescriptor shapeDescriptor, SnippetDescriptor snippetDescriptor) {
            var shapeType = shapeDescriptor.ShapeType;
            var shape = (dynamic)_shapeFactory.Value.Create(shapeType);

            shape.Element = context.Element;
            shape.SnippetDescriptor = snippetDescriptor;

            if (snippetDescriptor != null) {
                foreach (var fieldDescriptor in snippetDescriptor.Fields) {
                    var value = context.Element.Data.Get(fieldDescriptor.Name);
                    shape.Properties[fieldDescriptor.Name] = value;
                }
            }

            ElementShapes.AddTokenizers(shape, _tokenizer.Value);
            context.ElementShape.Snippet = shape;
        }

        private SnippetDescriptor CreateSnippetDescriptor(ShapeDescriptor shapeDescriptor, Snippet snippetElement) {
            // Initializing and checking access to the Snippet manifest file.
            var physicalSourcePath = _wca.GetContext().HttpContext.Server.MapPath(shapeDescriptor.BindingSource);
            var fullPath = Path.Combine(
                Path.GetDirectoryName(physicalSourcePath) ?? "",
                Path.GetFileNameWithoutExtension(physicalSourcePath) + ".txt");

            SnippetDescriptor descriptor;
            // Reading and parsing the manifest if it exists.
            if (File.Exists(fullPath)) {
                var deserializer = new DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .WithTypeConverter(new LocalizedStringYamlConverter())
                    .Build();

                descriptor = deserializer.Deserialize<SnippetDescriptor>(File.OpenText(fullPath));
            }
            // Otherwise extract the Fields from the Snippet shape template.
            else {
                var shape = (dynamic)_shapeFactory.Value.Create(shapeDescriptor.ShapeType);
                shape.Element = snippetElement;

                descriptor = new SnippetDescriptor();

                shape.DescriptorRegistrationCallback = (Action<SnippetFieldDescriptor>)(fieldDescriptor => {
                    // Not using Dictionary, as that will break rendering the view for some obscure reason.
                    var existingFieldDescriptor = descriptor.Fields.SingleOrDefault(field => field.Name == fieldDescriptor.Name);

                    if (existingFieldDescriptor == null)
                        descriptor.Fields.Add(fieldDescriptor);

                    if (fieldDescriptor.DisplayName == null)
                        fieldDescriptor.DisplayName = new LocalizedString(fieldDescriptor.Name);
                });

                using (_currentThemeShapeBindingResolver.Value.Enable()) {
                    _shapeDisplay.Value.Display(shape);
                }

                shape.SnippetDescriptor = descriptor;
            }

            // Checking the validity of the parsed values, include those of the Snippet's Fields.
            if (string.IsNullOrEmpty(descriptor.Category))
                descriptor.Category = snippetElement.Category;

            if (string.IsNullOrEmpty(descriptor.Description?.Text))
                descriptor.Description = T("An element that renders the {0} shape.", shapeDescriptor.ShapeType);

            if (string.IsNullOrEmpty(descriptor.DisplayName?.Text)) {
                var fileName = Path.GetFileNameWithoutExtension(shapeDescriptor.BindingSource) ?? "";
                var lastIndex = fileName.IndexOf(SnippetShapeSuffix, StringComparison.OrdinalIgnoreCase);
                descriptor.DisplayName = T(fileName.Substring(0, lastIndex).CamelFriendly());
            }

            if (string.IsNullOrEmpty(descriptor.ToolboxIcon))
                descriptor.ToolboxIcon = snippetElement.ToolboxIcon;

            descriptor.Fields = descriptor.Fields.Where(field => field.IsValid).ToList();

            return descriptor;
        }
    }
}