using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Reports;

namespace Orchard.Core.Reports.ViewModels {
    public class ReportsAdminIndexViewModel : BaseViewModel {
        public IList<Report> Reports { get; set; }
    }
}