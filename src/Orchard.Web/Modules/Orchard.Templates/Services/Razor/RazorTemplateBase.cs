using System.IO;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Orchard.Templates.Compilation.Razor {
    public interface IRazorTemplateBase {
        dynamic Model { get; }
        WebPageContext WebPageContext { get; set; }
        ViewContext ViewContext { get; set; }
        ViewDataDictionary ViewData { get; set; }
        WorkContext WorkContext { get; set; }
        string VirtualPath { get; set; }
        void Render(TextWriter writer);
        void InitHelpers();

    }

    public interface IRazorTemplateBase<TModel> : IRazorTemplateBase {
        new TModel Model { get; }
        new ViewDataDictionary<TModel> ViewData { get; set; }
    }

    public abstract class RazorTemplateBase<T> : Mvc.ViewEngines.Razor.WebViewPage<T>, IRazorTemplateBase<T> {
        public WebPageContext WebPageContext { get; set; }
        public virtual void Render(TextWriter writer) {
            InitHelpers();
            PushContext(WebPageContext, writer);
            OutputStack.Push(writer);
            Execute();
            OutputStack.Pop();
            PopContext();
        }
    }
}