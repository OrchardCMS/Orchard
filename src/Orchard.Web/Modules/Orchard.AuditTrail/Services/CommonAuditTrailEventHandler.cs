using System;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization.Services;

namespace Orchard.AuditTrail.Services {
    public class CommonAuditTrailEventHandler : AuditTrailEventHandlerBase {
        private readonly Lazy<IAuditTrailManager> _auditTrailManager;
        private readonly IDateServices _dateServices;

        public CommonAuditTrailEventHandler(Lazy<IAuditTrailManager> auditTrailManager, IDateServices dateServices) {
            _auditTrailManager = auditTrailManager;
            _dateServices = dateServices;
        }

        public override void Filter(QueryFilterContext context) {
            // Common filters (username, from and to).
            var userName = context.Filters.Get("username");
            var fromDate = context.Filters.Get("from.Date");
            var fromTime = context.Filters.Get("from.Time");
            var toDate = context.Filters.Get("to.Date");
            var toTime = context.Filters.Get("to.Time");
            var from = _dateServices.ConvertFromLocalString(fromDate, fromTime).Earliest();
            var to = _dateServices.ConvertFromLocalString(toDate, toTime).Latest();
            var query = context.Query;

            if (!String.IsNullOrWhiteSpace(userName)) query = query.Where(x => x.UserName == userName);
            if (from != null) query = query.Where(x => x.CreatedUtc >= from);
            if (to != null) query = query.Where(x => x.CreatedUtc <= to);

            context.Query = query;
        }

        public override void DisplayFilter(DisplayFilterContext context) {
            // Common filters (username, from and to).
            var userName = context.Filters.Get("username");
            var fromDate = context.Filters.Get("from.Date");
            var toDate = context.Filters.Get("to.Date");
            var userNameFilterDisplay = context.ShapeFactory.AuditTrailFilter__Common__User(UserName: userName);
            var dateFilterDisplay = context.ShapeFactory.AuditTrailFilter__Common__Date(
                From: new DateTimeEditor {Date = fromDate, ShowDate = true},
                To: new DateTimeEditor {Date = toDate, ShowDate = true});

            context.FilterLayout.TripleFirst.Add(dateFilterDisplay);
            context.FilterLayout.TripleSecond.Add(userNameFilterDisplay);
        }
    }
}