using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.WebPages;
using Orchard.Compilation.Razor;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.Themes.Models;

namespace Orchard.Templates.Services {
    public class RazorTemplateProcessor : TemplateProcessorImpl {
        private readonly IRazorCompiler _compiler;
        private readonly IRazorTemplateCache _cache;
        private readonly ISiteService _siteService;
        private readonly IHttpContextAccessor _hca;

        public override string Type {
            get { return "Razor"; }
        }

        public RazorTemplateProcessor(IRazorCompiler compiler, IRazorTemplateCache cache, ISiteService siteService, IHttpContextAccessor hca) {
            _compiler = compiler;
            _cache = cache;
            _siteService = siteService;
            _hca = hca;
            Logger = NullLogger.Instance;
        }

        ILogger Logger { get; set; }

        public override void Verify(string template) {
            _compiler.CompileRazor(template, null, new Dictionary<string, object>());
        }

        public override string Process(string template, string name, DisplayContext context = null, dynamic model = null) {
            if (String.IsNullOrEmpty(template))
                return string.Empty;

            var compiledTemplate = _compiler.CompileRazor(template, name, new Dictionary<string, object>());
            var result = ActivateAndRenderTemplate(compiledTemplate, context, null, model);
            return result;
        }

        private string ActivateAndRenderTemplate(IRazorTemplateBase obj, DisplayContext displayContext, string templateVirtualPath, object model) {
            var buffer = new StringBuilder(1024);
            using (var writer = new StringWriter(buffer)) {
                var htmlWriter = new HtmlTextWriter(writer);
                var httpContext = _hca.Current();

                // this should be done better - if no display context is provided we should fallback to current controller context somehow, if possible
                if (displayContext != null) {
                    var shapeViewContext = new ViewContext(displayContext.ViewContext.Controller.ControllerContext, displayContext.ViewContext.View, displayContext.ViewContext.ViewData, displayContext.ViewContext.TempData, htmlWriter);
                    obj.WebPageContext = new WebPageContext(displayContext.ViewContext.HttpContext, obj as WebPageRenderingBase, model);
                    obj.ViewContext = shapeViewContext;
                    obj.ViewData = new ViewDataDictionary(displayContext.ViewDataContainer.ViewData) { Model = model };
                }
                else {
                    obj.ViewData = new ViewDataDictionary(model);
                    obj.WebPageContext = new WebPageContext(httpContext, obj as WebPageRenderingBase, model);
                }

                obj.VirtualPath = templateVirtualPath ?? "~/Themes/" + _siteService.GetSiteSettings().As<ThemeSiteSettingsPart>().CurrentThemeName;
                obj.InitHelpers();
                obj.Render(htmlWriter);
            }

            return buffer.ToString();
        }
    }

}