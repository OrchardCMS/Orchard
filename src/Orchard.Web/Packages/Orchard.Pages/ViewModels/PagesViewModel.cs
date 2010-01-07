using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Pages.Models;

namespace Orchard.Pages.ViewModels {
    public class PagesViewModel : AdminViewModel {
        public IEnumerable<Page> Pages { get; set; }
    }
}