using Orchard.ContentManagement;

namespace IDeliverable.Slides.ViewModels
{
    public class ListSlidesProviderViewModel
    {
        public string SelectedListId { get; set; }
        public ContentItem SelectedList { get; set; }
        public string DisplayType { get; set; }
    }
}