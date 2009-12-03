using System;
using System.Collections.Generic;

namespace Orchard.Models.ViewModels {
    public class ItemDisplayViewModel {
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
        public IEnumerable<TemplateViewModel> Displays { get; set; }
    }

    public class ItemDisplayViewModel<TPart> : ItemDisplayViewModel where TPart : IContent {
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