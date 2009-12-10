using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.Models.ViewModels {
    public class ItemDisplayModel {
        private ContentItem _item;

        protected ItemDisplayModel() {
        }

        protected ItemDisplayModel(ItemDisplayModel displayModel) {
            TemplateName = displayModel.TemplateName;
            Prefix = displayModel.Prefix;
            Displays = displayModel.Displays.ToArray();
            Item = displayModel.Item;
        }

        public ItemDisplayModel(ContentItem item) {
            Item = item;
        }

        public ContentItem Item {
            get { return _item; }
            set { SetItem(value); }
        }

        protected virtual void SetItem(ContentItem value) {
            _item = value;
        }

        public Func<HtmlHelper, ItemDisplayModel, HtmlHelper> Adaptor { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public IEnumerable<TemplateViewModel> Displays { get; set; }
    }

    public class ItemDisplayModel<TPart> : ItemDisplayModel where TPart : IContent {
        private TPart _item;

        public ItemDisplayModel() {

        }
        public ItemDisplayModel(ItemDisplayModel displayModel)
            : base(displayModel) {
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