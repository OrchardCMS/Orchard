using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class BuildEditorModelContext {
        public BuildEditorModelContext(ItemEditorModel editorModel) {
            ContentItem = editorModel.Item;            
            EditorModel = editorModel;
        }

        public ContentItem ContentItem { get; set; }
        public ItemEditorModel EditorModel { get; set; }

        public void AddEditor(TemplateViewModel editor) {
            EditorModel.Editors = EditorModel.Editors.Concat(new[] { editor });
        }
    }
}