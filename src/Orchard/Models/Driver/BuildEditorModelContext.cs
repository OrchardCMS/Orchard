using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class BuildEditorModelContext {
        public BuildEditorModelContext(ItemEditorModel editorModel, string groupName) {
            ContentItem = editorModel.Item;
            GroupName = groupName;
            EditorModel = editorModel;
        }

        public ContentItem ContentItem { get; set; }
        public string GroupName { get; set; }
        public ItemEditorModel EditorModel { get; set; }

        public void AddEditor(TemplateViewModel editor) {
            EditorModel.Editors = EditorModel.Editors.Concat(new[] { editor });
        }
    }
}