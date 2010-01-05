using System.Linq;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentManagement.Handlers {
    public class BuildEditorModelContext {
        public BuildEditorModelContext(ItemEditorModel editorModel) {
            ContentItem = editorModel.Item;            
            EditorModel = editorModel;
        }

        public ContentItem ContentItem { get; set; }
        public ItemEditorModel EditorModel { get; set; }

        
        public void AddEditor(TemplateViewModel display) {
            //TEMP: (loudej) transition code - from TemplateViewMode to ZoneItem
            EditorModel.Zones.AddEditorPart(
                display.ZoneName + ":" + display.Position,
                display.Model,
                display.TemplateName,
                display.Prefix);
        }
    }
}