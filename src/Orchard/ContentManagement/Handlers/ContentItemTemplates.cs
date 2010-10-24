using System;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.ContentManagement.Handlers {
    public class ContentItemTemplates<TContent> : TemplateFilterBase<TContent> where TContent : class, IContent {
        private readonly string _templateName;
        // todo: (heskew) use _prefix?
        //private readonly string _prefix;
        private readonly string[] _displayTypes;
        private Action<UpdateEditorContext, IContent> _updater;

        public ContentItemTemplates(string templateName)
            : this(templateName, "") {

        }

        public ContentItemTemplates(string templateName, string displayTypes) {
            _templateName = templateName;
            _displayTypes = (displayTypes ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            _updater = (context, viewModel) => context.Updater.TryUpdateModel(viewModel, "", null, null);
        }

        protected override void BuildDisplayShape(BuildDisplayContext context, TContent instance) {
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

        protected override void BuildEditorShape(BuildEditorContext context, TContent instance) {
        }

        protected override void UpdateEditorShape(UpdateEditorContext context, TContent instance) {
        }

        public void Updater(Action<UpdateEditorContext, IContent> updater) {
            _updater = updater;
        }
    }
}