using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Wikis.Models;

namespace Orchard.Wikis.ViewModels {
    public class PageIndexViewModel : BaseViewModel {
        public IList<WikiPage> Pages { get; set; }
    } 
}
