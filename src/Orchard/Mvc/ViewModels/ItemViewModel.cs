using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.UI.Zones;

namespace Orchard.Mvc.ViewModels {
    public class ItemViewModel : IZoneContainer {
        private ContentItem _item;

        protected ItemViewModel(ItemViewModel viewModel) {
            TemplateName = viewModel.TemplateName;
            Prefix = viewModel.Prefix;
            Item = viewModel.Item;
            Zones = viewModel.Zones;
        }

        public ItemViewModel(ContentItem item) {
            Zones = new ZoneCollection();
            Item = item;
        }

        public ContentItem Item {
            get { return _item; }
            private set { SetItem(value); }
        }

        protected virtual void SetItem(ContentItem value) {
            _item = value;
        }

        public Func<HtmlHelper, ItemViewModel, HtmlHelper> Adaptor { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public ZoneCollection Zones { get; private set; }
    }

    public class ItemViewModel<TPart> : ItemViewModel where TPart : IContent {
        private TPart _item;


        public ItemViewModel(ItemViewModel viewModel)
            : base(viewModel) {
        }

        public ItemViewModel(TPart content)
            : base(content.ContentItem) {
        }

        public new TPart Item {
            get { return _item; }
        }

        protected override void SetItem(ContentItem value) {
            _item = value.As<TPart>();
            base.SetItem(value);
        }
    }
}