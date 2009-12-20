using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class ContentItemTemplates<TContent> : TemplateFilterBase<TContent> where TContent : class, IContent {
        // todo: (heskew) use _prefix?
        private readonly string _prefix;
        private readonly string[] _displayTypes;
        private Action<UpdateEditorModelContext, ItemEditorModel<TContent>> _updater;

        public ContentItemTemplates(params string[] displayTypes) {
            _displayTypes = displayTypes;
            _updater = (context, viewModel) => context.Updater.TryUpdateModel(viewModel, "", null, null);
        }

        protected override void BuildDisplayModel(BuildDisplayModelContext context, TContent instance) {
            var longestMatch = LongestMatch(context.DisplayType);
            context.DisplayModel.TemplateName = (!string.IsNullOrEmpty(context.TemplatePath) ? context.TemplatePath : typeof(TContent).Name) + "/" + longestMatch;
            context.DisplayModel.Prefix = _prefix;

            if (context.DisplayModel.GetType() != typeof(ItemDisplayModel<TContent>)) {
                context.DisplayModel.Adaptor = (html, viewModel) => {
                    return new HtmlHelper<ItemDisplayModel<TContent>>(
                        html.ViewContext,
                        new ViewDataContainer { ViewData = new ViewDataDictionary(new ItemDisplayModel<TContent>(viewModel)) },
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

        protected override void BuildEditorModel(BuildEditorModelContext context, TContent instance) {
            context.EditorModel.TemplateName = (!string.IsNullOrEmpty(context.TemplatePath) ? context.TemplatePath : typeof(TContent).Name) + "/Detail";
            context.EditorModel.Prefix = _prefix;
            if (context.EditorModel.GetType() != typeof(ItemEditorModel<TContent>)) {
                context.EditorModel.Adaptor = (html, viewModel) => {
                    return new HtmlHelper<ItemEditorModel<TContent>>(
                        html.ViewContext,
                        new ViewDataContainer { ViewData = new ViewDataDictionary(new ItemEditorModel<TContent>(viewModel)) },
                        html.RouteCollection);
                };
            }
        }

        protected override void UpdateEditorModel(UpdateEditorModelContext context, TContent instance) {
            if (context.EditorModel is ItemEditorModel<TContent>)
                _updater(context, (ItemEditorModel<TContent>)context.EditorModel);
            else
                _updater(context, new ItemEditorModel<TContent>(context.EditorModel));
            context.EditorModel.TemplateName = (!string.IsNullOrEmpty(context.TemplatePath) ? context.TemplatePath : typeof(TContent).Name) + "/Detail";
            context.EditorModel.Prefix = _prefix;
        }

        public void Updater(Action<UpdateEditorModelContext, ItemEditorModel<TContent>> updater) {
            _updater = updater;
        }
    }
}