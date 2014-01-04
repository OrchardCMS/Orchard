using System;
using System.Linq;
using System.Web;
using Orchard.Compilation.Razor;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions.Models;
using Orchard.Mvc.Spooling;

namespace Orchard.Templates.Services {
    public class TemplateBindingStrategy : IShapeTableProvider {
        private readonly ITemplateService _templateService;
        private readonly IRazorTemplateHolder _templateProvider;

        public TemplateBindingStrategy(
            ITemplateService templateService,
            IRazorTemplateHolder templateProvider) {
            _templateService = templateService;
            _templateProvider = templateProvider;
        }

        public virtual Feature Feature { get; set; }

        public void Discover(ShapeTableBuilder builder) {
            BuildShapes(builder);
        }

        private void BuildShapes(ShapeTableBuilder builder) {

            var shapes = _templateService.GetTemplates().Select(r =>
                new {
                    r.Name,
                    r.Language,
                    r.Template
                })
                .ToList();

            // Use a fake theme descriptor which will ensure the shape is used over
            // any other extension. It's also necessary to define them in the Admin 
            // theme in order to process tokens

            var fakeThemeDescriptor = new FeatureDescriptor {
                Id = "", // so that the binding is not filtered out
                Priority = 10, // so that it's higher than the themes' priority
                Extension = new ExtensionDescriptor {
                    ExtensionType = DefaultExtensionTypes.Theme, // so that the binding is overriding modules
                }
            };

            foreach (var record in shapes) {
                _templateProvider.Set(record.Name, record.Template);
                var shapeType = AdjustName(record.Name);

                builder.Describe(shapeType)
                       .From(new Feature { Descriptor = fakeThemeDescriptor })
                       .BoundAs("Template::" + shapeType,
                                descriptor => context => {
                                    var template = _templateProvider.Get(record.Name);
                                    return template != null ? PerformInvoke(context, record.Name, record.Language, template) : new HtmlString("");
                                });
            }
        }

        private IHtmlString PerformInvoke(DisplayContext displayContext, string name, string type, string template)
        {
            if (String.IsNullOrEmpty(template)) {
                return null;
            }

            var output = new HtmlStringWriter();
            output.Write(CoerceHtmlString(_templateService.Execute(template, name, type, displayContext, displayContext.Value)));

            return output;
        }

        private string AdjustName(string name) {
            var lastDash = name.LastIndexOf('-');
            var lastDot = name.LastIndexOf('.');
            if (lastDot <= 0 || lastDot < lastDash) {
                name = Adjust(name, null);
                return name;
            }

            var displayType = name.Substring(lastDot + 1);
            name = Adjust(name.Substring(0, lastDot), displayType);
            return name;
        }

        private static string Adjust(string name, string displayType) {
            // Canonical shape type names must not have - or . to be compatible with display and shape api calls.
            var shapeType = name.Replace("--", "__").Replace("-", "__").Replace('.', '_');

            if (String.IsNullOrEmpty(displayType)) {
                return shapeType;
            }
            var firstBreakingSeparator = shapeType.IndexOf("__", StringComparison.OrdinalIgnoreCase);
            if (firstBreakingSeparator <= 0) {
                return (shapeType + "_" + displayType);
            }

            return (shapeType.Substring(0, firstBreakingSeparator) + "_" + displayType + shapeType.Substring(firstBreakingSeparator));
        }

        private static IHtmlString CoerceHtmlString(object invoke) {
            return invoke as IHtmlString ?? (invoke != null ? new HtmlString(invoke.ToString()) : null);
        }

    }
}