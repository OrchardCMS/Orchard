using System;
using System.Linq;
using System.Web;
using Orchard.Caching;
using Orchard.Compilation.Razor;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Mvc.Spooling;
using Orchard.Settings;
using Orchard.Themes.Models;

namespace Orchard.Templates.Services {
    public class TemplateBindingStrategy : IShapeTableProvider {
        private readonly IWorkContextAccessor _wca;
        private readonly ICacheManager _cache;
        private readonly ISignals _signals;

        public TemplateBindingStrategy(IWorkContextAccessor wca, ICacheManager cache, ISignals signals) {
            _wca = wca;
            _cache = cache;
            _signals = signals;
        }

        public void Discover(ShapeTableBuilder builder) {
            EnsureWorkContext(() => BuildShapes(builder));
        }

        private void BuildShapes(ShapeTableBuilder builder) {
            
            var templateService = _wca.GetContext().Resolve<ITemplateService>();
            var templateCache = _wca.GetContext().Resolve<IRazorTemplateCache>();
            var siteService = _wca.GetContext().Resolve<ISiteService>();
            var extensionManager = _wca.GetContext().Resolve<IExtensionManager>();

            var currentTheme = extensionManager.GetExtension(siteService.GetSiteSettings().As<ThemeSiteSettingsPart>().CurrentThemeName);
            var themeFeature = currentTheme.Features.FirstOrDefault();

            var hackedDescriptor = new FeatureDescriptor
            {
                Category = themeFeature.Category,
                Dependencies = themeFeature.Dependencies,
                Description = themeFeature.Description,
                Extension = themeFeature.Extension,
                Id = themeFeature.Id,
                Name = themeFeature.Name,
                Priority = int.MaxValue
            };

            var shapes = _cache.Get(
                DefaultTemplateService.TemplatesSignal, 
                ctx => {
                    ctx.Monitor(_signals.When(DefaultTemplateService.TemplatesSignal));
                    return templateService
                        .GetTemplates()
                        .Select(r => new { 
                            r.Name, 
                            r.Language, 
                            r.Template })
                        .ToList();
                });

            foreach (var record in shapes) {
                templateCache.Set(record.Name, record.Template);
                var shapeType = AdjustName(record.Name);

                builder.Describe(shapeType)
                       .From(new Feature { Descriptor = hackedDescriptor })
                       .BoundAs("Template::" + shapeType,
                                descriptor => context => {
                                    var template = templateCache.Get(record.Name);
                                    return template != null ? PerformInvoke(context, record.Name, record.Language, template) : new HtmlString("");
                                });
            }
        }

        private IHtmlString PerformInvoke(DisplayContext displayContext, string name, string type, string template)
        {
            var service = _wca.GetContext().Resolve<ITemplateService>();
            var output = new HtmlStringWriter();

            if (String.IsNullOrEmpty(template))
                return null;

            output.Write(CoerceHtmlString(service.Execute(template, name, type, displayContext, displayContext.Value)));

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

        private void EnsureWorkContext(Action action) {
            var workContext = _wca.GetContext();
            if (workContext != null) {
                action();
            }
            else {
                using (_wca.CreateWorkContextScope()) {
                    action();
                }
            }
        }
    }
}