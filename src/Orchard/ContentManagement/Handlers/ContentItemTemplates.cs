using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentManagement.Handlers {
    public class ContentItemTemplates<TContent> : TemplateFilterBase<TContent> where TContent : class, IContent {
        private readonly string _templateName;
        // todo: (heskew) use _prefix?
        private readonly string _prefix;
        private readonly string[] _displayTypes;
        private Action<UpdateEditorModelContext, IContent> _updater;

        public ContentItemTemplates(string templateName)
            : this(templateName, "") {

        }

        public ContentItemTemplates(string templateName, string displayTypes) {
            _templateName = templateName;
            _displayTypes = (displayTypes ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            _updater = (context, viewModel) => context.Updater.TryUpdateModel(viewModel, "", null, null);
        }

        protected override void BuildDisplayShape(BuildDisplayModelContext context, TContent instance) {
#if REFACTORING
            context.ViewModel.TemplateName = _templateName;
            var longestMatch = LongestMatch(context.DisplayType);
            if (!string.IsNullOrEmpty(longestMatch))
                context.ViewModel.TemplateName += "." + longestMatch;

            context.ViewModel.Prefix = _prefix;

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

        private string LongestMatch(string displayType) {
            if (string.IsNullOrEmpty(displayType))
                return displayType;

            return _displayTypes.Aggregate("", (best, x) => {
                if (displayType.StartsWith(x) && x.Length > best.Length) return x;
                return best;
            });
        }

        protected override void BuildEditorShape(BuildEditorModelContext context, TContent instance) {
#if REFACTORING
            context.ViewModel.TemplateName = _templateName;
            context.ViewModel.Prefix = _prefix;
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

        protected override void UpdateEditorShape(UpdateEditorModelContext context, TContent instance) {
#if REFACTORING
            if (context.ViewModel is ContentItemViewModel<TContent>)
                _updater(context, (ContentItemViewModel<TContent>)context.ViewModel);
            else
                _updater(context, new ContentItemViewModel<TContent>(context.ViewModel));
            context.ViewModel.TemplateName = _templateName;
            context.ViewModel.Prefix = _prefix;
#endif
        }

        public void Updater(Action<UpdateEditorModelContext, IContent> updater) {
            _updater = updater;
        }
    }
}