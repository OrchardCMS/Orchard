using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.Models.ViewModels {
    public class ItemDisplayViewModel {
        private ContentItem _item;

        protected ItemDisplayViewModel() {
        }

        protected ItemDisplayViewModel(ItemDisplayViewModel viewModel) {
            TemplateName = viewModel.TemplateName;
            Prefix = viewModel.Prefix;
            Displays = viewModel.Displays.ToArray();
            Item = viewModel.Item;
        }

        public ContentItem Item {
            get { return _item; }
            set { SetItem(value); }
        }

        protected virtual void SetItem(ContentItem value) {
            _item = value;
        }

        public Func<HtmlHelper, ItemDisplayViewModel, HtmlHelper> Adaptor { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public IEnumerable<TemplateViewModel> Displays { get; set; }
    }

    public class ItemDisplayViewModel<TPart> : ItemDisplayViewModel where TPart : IContent {
        private TPart _item;

        public ItemDisplayViewModel() {

        }
        public ItemDisplayViewModel(ItemDisplayViewModel viewModel)
            : base(viewModel) {
        }

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