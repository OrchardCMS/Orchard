using Orchard.ContentManagement.Drivers;
using Orchard.Search.Models;
using Orchard.Search.ViewModels;

namespace Orchard.Search.Drivers {
    public class SearchFormPartDriver : ContentPartDriver<SearchFormPart> {

        protected override DriverResult Display(SearchFormPart part, string displayType, dynamic shapeHelper) {
            var model = new SearchViewModel();
            return ContentPartTemplate(model, "Parts/Search.SearchForm");
        }
    }
}