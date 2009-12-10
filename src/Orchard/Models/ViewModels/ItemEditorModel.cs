using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.Models.ViewModels {
    public class ItemEditorModel {
        private ContentItem _item;

        protected ItemEditorModel() {
        }

        protected ItemEditorModel(ItemEditorModel editorModel) {
            TemplateName = editorModel.TemplateName;
            Prefix = editorModel.Prefix;
            Editors = editorModel.Editors.ToArray();
            Item = editorModel.Item;
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
        public IEnumerable<TemplateViewModel> Editors { get; set; }
    }

    public class ItemEditorModel<TPart> : ItemEditorModel where TPart : IContent {
        private TPart _item;


        public ItemEditorModel() {

        }
        public ItemEditorModel(ItemEditorModel editorModel)
            : base(editorModel) {
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
