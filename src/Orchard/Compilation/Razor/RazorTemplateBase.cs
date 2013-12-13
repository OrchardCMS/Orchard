using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.WebPages;
using System.Web.WebPages.Instrumentation;

namespace Orchard.Compilation.Razor {
    public interface IRazorTemplateBase
    {
        dynamic Model { get; }
        WebPageContext WebPageContext { get; set; }
        ViewContext ViewContext { get; set; }
        ViewDataDictionary ViewData { get; set; }
        string VirtualPath { get; set; }
        void Render(TextWriter writer);
        void InitHelpers();

    }

    public interface IRazorTemplateBase<TModel> : IRazorTemplateBase
    {
        TModel Model { get; }
        ViewDataDictionary<TModel> ViewData { get; set; }
    }

    public abstract class RazorTemplateBase<T> : Mvc.ViewEngines.Razor.WebViewPage<T>, IRazorTemplateBase<T> {
        public WebPageContext WebPageContext { get; set; }
        public void Render(TextWriter writer) {
            PushContext(WebPageContext, writer);
            OutputStack.Push(writer);
            Execute();
            OutputStack.Pop();
            PopContext();
        }
    }
}