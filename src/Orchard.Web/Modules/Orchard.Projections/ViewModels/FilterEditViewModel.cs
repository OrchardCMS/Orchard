using Orchard.Projections.Descriptors.Filter;

namespace Orchard.Projections.ViewModels {

    public class FilterEditViewModel {
        public int Id { get; set; }
        public string Description { get; set; }
        public FilterDescriptor Filter { get; set; }
        public dynamic Form { get; set; }
    }
}
