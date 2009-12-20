using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class BuildEditorModelContext {
        public BuildEditorModelContext(ItemEditorModel editorModel, string groupName, string templatePath) {
            ContentItem = editorModel.Item;
            GroupName = groupName;
            EditorModel = editorModel;
            TemplatePath = templatePath;
        }

        public ContentItem ContentItem { get; private set; }
        public string GroupName { get; private set; }
        public ItemEditorModel EditorModel { get; private set; }
        public string TemplatePath { get; private set; }

        public void AddEditor(TemplateViewModel editor) {
            EditorModel.Editors = EditorModel.Editors.Concat(new[] { editor });
        }
    }
}