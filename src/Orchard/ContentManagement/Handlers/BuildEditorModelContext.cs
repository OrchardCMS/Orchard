using Orchard.ContentManagement.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentManagement.Handlers {
    public class BuildEditorModelContext {
        public BuildEditorModelContext(ContentItemViewModel viewModel) {
            ContentItem = viewModel.Item;            
            ViewModel = viewModel;
        }

        public ContentItem ContentItem { get; set; }
        public ContentItemViewModel ViewModel { get; set; }

        
        public void AddEditor(TemplateViewModel display) {
            //TEMP: (loudej) transition code - from TemplateViewMode to ZoneItem
            ViewModel.Zones.AddEditorPart(
                display.ZoneName + ":" + display.Position,
                display.Model,
                display.TemplateName,
                display.Prefix);
        }
    }
}