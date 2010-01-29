using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Core.Themes.ViewModels {
    public class PreviewViewModel {
        public IEnumerable<SelectListItem> Themes { get; set; }
    }
}
