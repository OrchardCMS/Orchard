using System;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class ContentItemTemplates<TContent> : TemplateFilterBase<TContent> where TContent : class, IContent {
        private readonly string _templateName;
        private readonly string _prefix;
        private readonly string[] _displayTypes;
        private Action<UpdateContentContext, ItemEditorViewModel<TContent>> _updater;

        public ContentItemTemplates(string templateName, params string[] displayTypes) {
            _templateName = templateName;
            _displayTypes = displayTypes;
            _updater = (context, viewModel) => context.Updater.TryUpdateModel(viewModel, "", null, null);
        }

        protected override void GetDisplays(GetDisplaysContext context, TContent instance) {
            var longestMatch = LongestMatch(context.DisplayType);
            context.ItemView.TemplateName = _templateName + longestMatch;
            context.ItemView.Prefix = _prefix;
        }

        private string LongestMatch(string displayType) {
            return _displayTypes.Aggregate("", (best, x) => {
                                                   if (displayType.StartsWith(x) && x.Length > best.Length) return x;
                                                   return best;
                                               });
        }

        protected override void GetEditors(GetEditorsContext context, TContent instance) {
            context.ItemView.TemplateName = _templateName;
            context.ItemView.Prefix = _prefix;
        }

        protected override void UpdateEditors(UpdateContentContext context, TContent instance) {
            _updater(context, (ItemEditorViewModel<TContent>)context.ItemView);
            context.ItemView.TemplateName = _templateName;
            context.ItemView.Prefix = _prefix;
        }

        public void Updater(Action<UpdateContentContext, ItemEditorViewModel<TContent>> updater) {
            _updater = updater;
        }
    }
}