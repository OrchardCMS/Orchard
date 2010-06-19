using System;
using Orchard.Mvc.ViewModels;

namespace Orchard.Indexing.ViewModels {
    public class IndexViewModel : BaseViewModel {
        public bool HasIndexToManage { get; set; }
        //todo: hang the index updated date off here to show in the admin UI (e.g. -> index updated: June 4, 2010 [update index])
        public DateTime IndexUpdatedUtc { get; set; }
    }
}