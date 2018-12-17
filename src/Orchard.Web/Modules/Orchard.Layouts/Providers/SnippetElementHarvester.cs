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
using Orchard.Layouts.Services;
using Orchard.Layouts.Shapes;
using Orchard.Layouts.ViewModels;
using Orchard.Localization;
using Orchard.Themes.Services;
using Orchard.Tokens;
using Orchard.Utility.Extensions;
using YamlDotNet.RepresentationModel;

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
                var snippetDescriptor = ParseSnippetManifest(shapeDescriptor.Value, snippetElement);

                if (snippetDescriptor == null) continue;

                var shapeType = shapeDescriptor.Value.ShapeType;

                yield return new ElementDescriptor(elementType, shapeType, snippetDescriptor.DisplayName, snippetDescriptor.Description, snippetDescriptor.Category) {
                    Displaying = displayContext => Displaying(displayContext, shapeDescriptor.Value, snippetDescriptor),
                    ToolboxIcon = snippetDescriptor.ToolboxIcon,
                    EnableEditorDialog = snippetDescriptor.Fields.Any(),
                    Editor = ctx => Editor(snippetDescriptor ?? DescribeSnippet(shapeType, snippetElement), ctx),
                    UpdateEditor = ctx => UpdateEditor(snippetDescriptor ?? DescribeSnippet(shapeType, snippetElement), ctx)
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
                var fieldEditorTemplateName = String.Format("Elements.Snippet.Field.{0}", x.Type ?? "Text");
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

        private SnippetDescriptor ParseSnippetManifest(ShapeDescriptor shape, Snippet snippetElement) {
            // Initializing and checking access to the Snippet manifest file.
            var physicalSourcePath = _wca.GetContext().HttpContext.Server.MapPath(shape.BindingSource);
            var fullPath = Path.Combine(
                Path.GetDirectoryName(physicalSourcePath) ?? "",
                Path.GetFileNameWithoutExtension(physicalSourcePath) + ".txt");

            if (!File.Exists(fullPath)) return null;

            // Reading and parsing the manifest.
            var manifestText = File.ReadAllText(fullPath);
            var yaml = new YamlStream();

            yaml.Load(new StringReader(manifestText));
            var manifest = (YamlMappingNode)yaml.Documents?[0]?.RootNode;

            if (!manifest?.Children?.Any() ?? true) return null;

            // Extracting the main properties of the manifest.
            var category = GetYamlMappingNodeChildByName(manifest, nameof(SnippetDescriptor.Category))?.ToString();
            if (string.IsNullOrEmpty(category)) category = snippetElement.Category;

            var description = GetYamlMappingNodeChildByName(manifest, nameof(SnippetDescriptor.Description))?.ToString();
            if (string.IsNullOrEmpty(description)) description = $"An element that renders the {shape.ShapeType} shape.";

            var displayName = GetYamlMappingNodeChildByName(manifest, nameof(SnippetDescriptor.DisplayName))?.ToString();
            if (string.IsNullOrEmpty(displayName)) {
                var fileName = Path.GetFileNameWithoutExtension(shape.BindingSource) ?? "";
                var lastIndex = fileName.IndexOf(SnippetShapeSuffix, StringComparison.OrdinalIgnoreCase);
                displayName = fileName.Substring(0, lastIndex).CamelFriendly();
            }

            var toolboxIcon = GetYamlMappingNodeChildByName(manifest, nameof(SnippetDescriptor.ToolboxIcon))?.ToString();
            if (string.IsNullOrEmpty(toolboxIcon)) toolboxIcon = snippetElement.ToolboxIcon;

            var descriptor = new SnippetDescriptor {
                Category = category,
                Description = new LocalizedString(description),
                DisplayName = new LocalizedString(displayName),
                ToolboxIcon = toolboxIcon
            };

            // Extracting the editor fields from the manifest.
            var fields = (YamlSequenceNode)GetYamlMappingNodeChildByName(manifest, nameof(SnippetDescriptor.Fields));

            if (fields?.Any() ?? false) {
                foreach (YamlMappingNode field in fields) {
                    descriptor.Fields.Add(new SnippetFieldDescriptor {
                        Description = new LocalizedString(GetYamlMappingNodeChildByName(field, nameof(SnippetFieldDescriptor.Description))?.ToString() ?? ""),
                        DisplayName = new LocalizedString(GetYamlMappingNodeChildByName(field, nameof(SnippetFieldDescriptor.DisplayName))?.ToString() ?? ""),
                        Name = GetYamlMappingNodeChildByName(field, nameof(SnippetFieldDescriptor.Name))?.ToString(),
                        Type = GetYamlMappingNodeChildByName(field, nameof(SnippetFieldDescriptor.Type))?.ToString()
                    });
                }

                descriptor.Fields = descriptor.Fields.Where(field => field.IsValid).ToList();
            }

            return descriptor;
        }

        private YamlNode GetYamlMappingNodeChildByName(YamlMappingNode parent, string childNodeName) {
            YamlNode yamlNode = null;
            parent?.Children?.TryGetValue(new YamlScalarNode(childNodeName), out yamlNode);

            return yamlNode;
        }

        private SnippetDescriptor DescribeSnippet(string shapeType, Snippet element) {
            var shape = (dynamic)_shapeFactory.Value.Create(shapeType);
            shape.Element = element;
            return DescribeSnippet(shape);
        }

        private SnippetDescriptor DescribeSnippet(dynamic shape) {
            // Execute the shape and intercept calls to the Html.SnippetField method.
            var descriptor = new SnippetDescriptor();
            shape.DescriptorRegistrationCallback = (Action<SnippetFieldDescriptor>)(fieldDescriptor => {
                // Not using Dictionary, as that will break rendering the view for some obscure reason.
                var existingDescriptor = descriptor.Fields.SingleOrDefault(x => x.Name == fieldDescriptor.Name);

                if (existingDescriptor == null)
                    descriptor.Fields.Add(fieldDescriptor);

                if (fieldDescriptor.DisplayName == null)
                    fieldDescriptor.DisplayName = new LocalizedString(fieldDescriptor.Name);
            });

            using (_currentThemeShapeBindingResolver.Value.Enable()) {
                _shapeDisplay.Value.Display(shape);
            }

            shape.SnippetDescriptor = descriptor;
            return descriptor;
        }
    }
}