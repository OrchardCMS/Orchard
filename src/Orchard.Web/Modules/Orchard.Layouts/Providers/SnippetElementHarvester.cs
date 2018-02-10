using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
using YamlDotNet.Dynamic;

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
            var shapeDescriptors = shapeTable.Bindings.Where(x => !String.Equals(x.Key, "Elements_Snippet", StringComparison.OrdinalIgnoreCase) && x.Key.EndsWith(SnippetShapeSuffix, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value.ShapeDescriptor);
            var elementType = typeof(Snippet);
            var snippetElement = (Snippet)_elementFactory.Value.Activate(elementType);

            foreach (var shapeDescriptor in shapeDescriptors) {
                var snippetManifest = ParseSnippetManifest(shapeDescriptor.Value.BindingSource);
                var shapeType = shapeDescriptor.Value.ShapeType;
                var elementName = GetDisplayName(snippetManifest, shapeDescriptor.Value.BindingSource);
                var toolboxIcon = GetToolboxIcon(snippetManifest, snippetElement);
                var description = GetDescription(snippetManifest, shapeType);
                var category = GetCategory(snippetManifest, snippetElement);
                var closureDescriptor = shapeDescriptor;
                var snippetDescriptor = ParseSnippetDescriptor(snippetManifest);
                yield return new ElementDescriptor(elementType, shapeType, new LocalizedString(elementName), description, category) {
                    Displaying = displayContext => Displaying(displayContext, closureDescriptor.Value, snippetDescriptor),
                    ToolboxIcon = toolboxIcon,
                    EnableEditorDialog = snippetDescriptor != null || HasSnippetFields(shapeDescriptor.Value),
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

        private dynamic ParseSnippetManifest(string bindingSource) {
            var physicalSourcePath = _wca.GetContext().HttpContext.Server.MapPath(bindingSource);
            var paramsFileName = Path.Combine(Path.GetDirectoryName(physicalSourcePath) ?? "", Path.GetFileNameWithoutExtension(physicalSourcePath) + ".txt");

            if (!File.Exists(paramsFileName))
                return null;

            var yaml = File.ReadAllText(paramsFileName);
            var snippetConfig = Deserialize(yaml);

            return snippetConfig;
        }

        private string GetDisplayName(dynamic snippetManifest, string bindingSource) {
            if (snippetManifest != null && (string)snippetManifest.DisplayName != null) {
                return (string)snippetManifest.DisplayName;
            }

            var fileName = Path.GetFileNameWithoutExtension(bindingSource) ?? "";
            var lastIndex = fileName.IndexOf(SnippetShapeSuffix, StringComparison.OrdinalIgnoreCase);
            return fileName.Substring(0, lastIndex).CamelFriendly();
        }

        private string GetToolboxIcon(dynamic snippetManifest, Snippet snippetElement) {
            if (snippetManifest != null && (string)snippetManifest.ToolboxIcon != null) {
                return Regex.Unescape((string)snippetManifest.ToolboxIcon);
            }

            return snippetElement.ToolboxIcon;
        }

        private LocalizedString GetDescription(dynamic snippetManifest, string shapeType) {
            if (snippetManifest != null && (string)snippetManifest.Description != null) {
                return new LocalizedString((string)snippetManifest.Description);
            }

            return new LocalizedString(String.Format("An element that renders the {0} shape.", shapeType));
        }

        private string GetCategory(dynamic snippetManifest, Snippet snippetElement) {
            if (snippetManifest != null && (string)snippetManifest.Category != null) {
                return (string)snippetManifest.Category;
            }

            return snippetElement.Category;
        }

        private SnippetDescriptor ParseSnippetDescriptor(dynamic snippetManifest) {
            if(snippetManifest == null || snippetManifest.Fields.Count == 0) {
                return null;
            }

            var fieldsConfig = snippetManifest.Fields.Children;
            var descriptor = new SnippetDescriptor();

            foreach (var fieldConfig in fieldsConfig) {
                descriptor.Fields.Add(new SnippetFieldDescriptor {
                    Name = (string)fieldConfig.Name,
                    Type = (string)fieldConfig.Type,
                    DisplayName = new LocalizedString((string)fieldConfig.DisplayName),
                    Description = new LocalizedString((string)fieldConfig.Description)
                });
            }

            return descriptor;
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
                var existingDescriptor = descriptor.Fields.SingleOrDefault(x => x.Name == fieldDescriptor.Name); // Not using Dictionary, as that will break rendering the view for some obscure reason.

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

        private bool HasSnippetFields(ShapeDescriptor shapeDescriptor) {
            var bindingSource = shapeDescriptor.BindingSource;
            var localFileName = _wca.GetContext().HttpContext.Server.MapPath(bindingSource);

            if (!File.Exists(localFileName))
                return false;

            var markup = File.ReadAllText(localFileName);
            return markup.Contains("@Html.SnippetField");
        }

        private dynamic Deserialize(string yaml) {
            return new DynamicYaml(yaml);
        }
    }
}