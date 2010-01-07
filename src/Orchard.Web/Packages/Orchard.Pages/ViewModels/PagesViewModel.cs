using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Pages.Models;

namespace Orchard.Pages.ViewModels {
    public class PagesViewModel : BaseViewModel {
        public IEnumerable<Page> Pages { get; set; }
    }
}