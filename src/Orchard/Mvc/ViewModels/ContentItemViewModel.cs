using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.UI.Zones;

namespace Orchard.Mvc.ViewModels {
#if REFACTORING
    public class ContentItemViewModel : IZoneContainer {
        private ContentItem _item;

        public ContentItemViewModel() {
            Zones = new ZoneCollection();
        }

        public ContentItemViewModel(ContentItem item) {
            Zones = new ZoneCollection();
            Item = item;
        }

        public ContentItemViewModel(ContentItemViewModel viewModel) {
            TemplateName = viewModel.TemplateName;
            Prefix = viewModel.Prefix;
            Item = viewModel.Item;
            Zones = viewModel.Zones;
        }

        public ContentItem Item {
            get { return _item; }
            private set { SetItem(value); }
        }

        protected virtual void SetItem(ContentItem value) {
            _item = value;
        }

        public Func<HtmlHelper, ContentItemViewModel, HtmlHelper> Adaptor { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public ZoneCollection Zones { get; private set; }
        public bool IsPublished {
            get { return Item != null && Item.VersionRecord != null && Item.VersionRecord.Published; }
        }
        public bool IsLatest {
            get { return Item != null && Item.VersionRecord != null && Item.VersionRecord.Latest; }
        }
        public bool IsDraft {
            get { return IsLatest && !IsPublished; }
        }
    }

    public class ContentItemViewModel<TPart> : ContentItemViewModel where TPart : IContent {
        private TPart _item;

        public ContentItemViewModel() {
        }

        public ContentItemViewModel(TPart content)
            : base(content.ContentItem) {
        }

        public ContentItemViewModel(ContentItemViewModel viewModel)
            : base(viewModel) {
        }

        public new TPart Item {
            get { return _item; }
        }

        protected override void SetItem(ContentItem value) {
            _item = value.As<TPart>();
            base.SetItem(value);
        }
    }
#endif
}