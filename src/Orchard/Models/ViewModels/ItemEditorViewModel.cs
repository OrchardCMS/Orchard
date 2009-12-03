using System.Collections.Generic;

namespace Orchard.Models.ViewModels {
    public class ItemEditorViewModel  {
        private ContentItem _item;
        public ContentItem Item {
            get { return _item; }
            set { SetItem(value); }
        }

        protected virtual void SetItem(ContentItem value) {
            _item = value;
        }

        public string TemplateName { get; set; }
        public string Prefix { get; set; }

        public IEnumerable<TemplateViewModel> Editors { get; set; }
    }

    public class ItemEditorViewModel<TPart> : ItemEditorViewModel where TPart : IContent {
        private TPart _item;
        public new TPart Item {
            get { return _item; }
            set { SetItem(value.ContentItem); }
        }

        protected override void SetItem(ContentItem value) {
            _item = value.As<TPart>();
            base.SetItem(value);
        }
    }
}