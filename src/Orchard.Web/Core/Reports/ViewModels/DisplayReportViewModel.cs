using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Reports;

namespace Orchard.Core.Reports.ViewModels {
    public class DisplayReportViewModel : BaseViewModel {
        public Report Report { get; set; }
    }
}