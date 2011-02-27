using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.ViewModels {
    public class MenuItemEntry {
        public int MenuItemId { get; set; }
        public bool IsMenuItem { get; set; }
        
        public string Text { get; set; }
        public string Url { get; set; }
        public string Position { get; set; }
    }
}