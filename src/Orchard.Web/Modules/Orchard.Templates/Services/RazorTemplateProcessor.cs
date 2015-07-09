using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.WebPages;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Templates.Compilation.Razor;

namespace Orchard.Templates.Services {
    [OrchardFeature("Orchard.Templates.Razor")]
    public class RazorTemplateProcessor : TemplateProcessorImpl {
        private readonly IRazorCompiler _compiler;
        private readonly IWorkContextAccessor _wca;

        public override string Type {
            get { return "Razor"; }
        }

        public RazorTemplateProcessor(
            IRazorCompiler compiler,
            IWorkContextAccessor wca) {

            _compiler = compiler;
            _wca = wca;
            Logger = NullLogger.Instance;
        }

        ILogger Logger { get; set; }

        public override void Verify(string template) {
            _compiler.CompileRazor(template, null, new Dictionary<string, object>());
        }

        public override string Process(string template, string name, DisplayContext context = null, dynamic model = null) {
            if (String.IsNullOrEmpty(template))
                return String.Empty;

            var compiledTemplate = _compiler.CompileRazor(template, name, new Dictionary<string, object>());
            var result = ActivateAndRenderTemplate(compiledTemplate, context, null, model);
            return result;
        }

        private string ActivateAndRenderTemplate(IRazorTemplateBase obj, DisplayContext displayContext, string templateVirtualPath, object model) {
            var buffer = new StringBuilder(1024);
            using (var writer = new StringWriter(buffer)) {
                using (var htmlWriter = new HtmlTextWriter(writer)) {

                    if (displayContext != null && displayContext.ViewContext.Controller != null) {
                        var shapeViewContext = new ViewContext(
                            displayContext.ViewContext.Controller.ControllerContext,
                            displayContext.ViewContext.View,
                            displayContext.ViewContext.ViewData,
                            displayContext.ViewContext.TempData,
                            htmlWriter
                            );

                        obj.WebPageContext = new WebPageContext(displayContext.ViewContext.HttpContext, obj as WebPageRenderingBase, model);
                        obj.ViewContext = shapeViewContext;

                        obj.ViewData = new ViewDataDictionary(displayContext.ViewDataContainer.ViewData) { Model = model };
                        obj.InitHelpers();
                    }
                    else {

                        // Setup a fake view context in order to support razor syntax inside of HTML attributes,
                        // for instance: <a href="@WorkContext.CurrentSite.BaseUrl">Homepage</a>.
                        var viewData = new ViewDataDictionary(model);
                        var httpContext = _wca.GetContext().HttpContext;
                        obj.ViewContext = new ViewContext(
                            new ControllerContext(
                                httpContext.Request.RequestContext,
                                new StubController()),
                                new StubView(),
                                viewData,
                                new TempDataDictionary(),
                                htmlWriter);

                        obj.ViewData = viewData;
                        obj.WebPageContext = new WebPageContext(httpContext, obj as WebPageRenderingBase, model);
                        obj.WorkContext = _wca.GetContext();
                    }

                    obj.VirtualPath = templateVirtualPath ?? "~/Themes/Orchard.Templates";
                    obj.Render(htmlWriter);
                }
            }

            return buffer.ToString();
        }

        private class StubController : Controller { }

        private class StubView : IView {
            public void Render(ViewContext viewContext, TextWriter writer) { }
        }
    }
}