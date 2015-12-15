using Orchard.MediaProcessing.Descriptors.Filter;

namespace Orchard.MediaProcessing.ViewModels {
    public class FilterEditViewModel {
        public int Id { get; set; }
        public string Description { get; set; }
        public FilterDescriptor Filter { get; set; }
        public dynamic Form { get; set; }
    }
}
