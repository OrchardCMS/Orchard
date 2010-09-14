using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class ContentItemTemplateResult<TContent> : DriverResult where TContent : class, IContent {
        public ContentItemTemplateResult(string templateName) {
            TemplateName = templateName;
        }

        public string TemplateName { get; set; }

        public override void Apply(BuildDisplayModelContext context) {
#if REFACTORING
            //todo: (heskew)evaluate - for lack of having access to the full context in a driver to conditionally return results (e.g. BlogDriver item display is otherwise being overriden by the ContentPartDriver)
            if (!string.IsNullOrWhiteSpace(context.ViewModel.TemplateName)
                && context.ViewModel.GetType() != typeof(ContentItemViewModel<TContent>))
                return;

            context.ViewModel.TemplateName = TemplateName;
            if (context.ViewModel.GetType() != typeof(ContentItemViewModel<TContent>)) {
                context.ViewModel.Adaptor = (html, viewModel) => {
                    return new HtmlHelper<ContentItemViewModel<TContent>>(
                        html.ViewContext,
                        new ViewDataContainer { ViewData = new ViewDataDictionary(new ContentItemViewModel<TContent>(viewModel)) },
                        html.RouteCollection);
                };
            }
#endif
        }

        public override void Apply(BuildEditorModelContext context) {
#if REFACTORING
            context.ViewModel.TemplateName = TemplateName;
            if (context.ViewModel.GetType() != typeof(ContentItemViewModel<TContent>)) {
                context.ViewModel.Adaptor = (html, viewModel) => {
                    return new HtmlHelper<ContentItemViewModel<TContent>>(
                        html.ViewContext,
                        new ViewDataContainer { ViewData = new ViewDataDictionary(new ContentItemViewModel<TContent>(viewModel)) },
                        html.RouteCollection);
                };
            }
#endif
        }

        class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }
        }

        public ContentItemTemplateResult<TContent> LongestMatch(string displayType, params string[] knownDisplayTypes) {

            if (string.IsNullOrEmpty(displayType))
                return this;

            var longest = knownDisplayTypes.Aggregate("", (best, x) => {
                if (displayType.StartsWith(x) && x.Length > best.Length) return x;
                return best;
            });

            if (string.IsNullOrEmpty(longest))
                return this;

            TemplateName += "." + longest;
            return this;
        }
    }
}