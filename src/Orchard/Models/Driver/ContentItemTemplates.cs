using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class ContentItemTemplates<TContent> : TemplateFilterBase<TContent> where TContent : class, IContent {
        private readonly string _templateName;
        private readonly string _prefix;
        private readonly string[] _displayTypes;
        private Action<UpdateEditorViewModelContext, ItemEditorViewModel<TContent>> _updater;

        public ContentItemTemplates(string templateName, params string[] displayTypes) {
            _templateName = templateName;
            _displayTypes = displayTypes;
            _updater = (context, viewModel) => context.Updater.TryUpdateModel(viewModel, "", null, null);
        }

        protected override void GetDisplayViewModel(GetDisplayViewModelContext context, TContent instance) {
            var longestMatch = LongestMatch(context.DisplayType);
            context.ViewModel.TemplateName = _templateName + longestMatch;
            context.ViewModel.Prefix = _prefix;

            if (context.ViewModel.GetType() != typeof(ItemDisplayViewModel<TContent>)) {
                context.ViewModel.Adaptor = (html, viewModel) => {
                    return new HtmlHelper<ItemDisplayViewModel<TContent>>(
                        html.ViewContext,
                        new ViewDataContainer { ViewData = new ViewDataDictionary(new ItemDisplayViewModel<TContent>(viewModel)) },
                        html.RouteCollection);
                };
            }
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

        protected override void GetEditorViewModel(GetEditorViewModelContext context, TContent instance) {
            context.ViewModel.TemplateName = _templateName;
            context.ViewModel.Prefix = _prefix;
        }

        protected override void UpdateEditorViewModel(UpdateEditorViewModelContext context, TContent instance) {
            _updater(context, (ItemEditorViewModel<TContent>)context.ViewModel);
            context.ViewModel.TemplateName = _templateName;
            context.ViewModel.Prefix = _prefix;
        }

        public void Updater(Action<UpdateEditorViewModelContext, ItemEditorViewModel<TContent>> updater) {
            _updater = updater;
        }
    }
}