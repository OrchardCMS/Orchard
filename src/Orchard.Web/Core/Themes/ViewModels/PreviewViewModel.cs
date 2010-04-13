using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.Core.Themes.ViewModels {
    public class PreviewViewModel {
        public IEnumerable<SelectListItem> Themes { get; set; }
    }
}
