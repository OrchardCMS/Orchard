using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.UI.Zones;

namespace Orchard.ContentManagement.ViewModels {
    public class ItemEditorModel : IZoneContainer {
        private ContentItem _item;

        protected ItemEditorModel() {
            Zones = new ZoneCollection();
        }

        protected ItemEditorModel(ItemEditorModel editorModel) {
            TemplateName = editorModel.TemplateName;
            Prefix = editorModel.Prefix;
            Item = editorModel.Item;
            Zones = editorModel.Zones;
        }

        public ItemEditorModel(ContentItem item) {
            Zones = new ZoneCollection();
            Item = item;
        }

        public ContentItem Item {
            get { return _item; }
            set { SetItem(value); }
        }

        protected virtual void SetItem(ContentItem value) {
            _item = value;
        }

        public Func<HtmlHelper, ItemEditorModel, HtmlHelper> Adaptor { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public ZoneCollection Zones { get; set; }
    }

    public class ItemEditorModel<TPart> : ItemEditorModel where TPart : IContent {
        private TPart _item;


        public ItemEditorModel() {

        }

        public ItemEditorModel(ItemEditorModel editorModel)
            : base(editorModel) {
        }

        public ItemEditorModel(TPart content)
            : base(content.ContentItem) {
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
