using System;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization.Services;

namespace Orchard.AuditTrail.Services {
    public class CommonAuditTrailEventHandler : AuditTrailEventHandlerBase {
        private readonly Lazy<IAuditTrailManager> _auditTrailManager;
        private readonly IDateLocalizationServices _dateLocalizationServices;

        public CommonAuditTrailEventHandler(Lazy<IAuditTrailManager> auditTrailManager, IDateLocalizationServices dateLocalizationServices) {
            _auditTrailManager = auditTrailManager;
            _dateLocalizationServices = dateLocalizationServices;
        }

        public override void Filter(QueryFilterContext context) {
            var userName = context.Filters.Get("username");
            var category = context.Filters.Get("category");
            var from = GetDateFromFilter(context.Filters, "From", "from").StartOfDay();
            var to = GetDateFromFilter(context.Filters, "To", "to").EndOfDay();
            var query = context.Query;

            if (!String.IsNullOrWhiteSpace(userName)) {
                query = query.Where(x => x.UserName == userName);
            }
            if (!String.IsNullOrWhiteSpace(category)) {
                query = query.Where(x => x.Category == category);
            }
            if (from != null) {
                query = query.Where(x => x.CreatedUtc >= from);
            }
            if (to != null) {
                query = query.Where(x => x.CreatedUtc <= to);
            }

            context.Query = query;
        }

        public override void DisplayFilter(DisplayFilterContext context) {
            var userName = context.Filters.Get("username");
            var fromDate = context.Filters.Get("from.Date");
            var toDate = context.Filters.Get("to.Date");
            var category = context.Filters.Get("category");
            var userNameFilterDisplay = context.ShapeFactory.AuditTrailFilter__Common__User(UserName: userName);
            var dateFromFilterDisplay = context.ShapeFactory.AuditTrailFilter__Common__Date__From(Editor: new DateTimeEditor {Date = fromDate, ShowDate = true});
            var dateToFilterDisplay = context.ShapeFactory.AuditTrailFilter__Common__Date__To(Editor: new DateTimeEditor { Date = toDate, ShowDate = true }); 
            var categoryFilterDisplay = context.ShapeFactory.AuditTrailFilter__Common__Category(
                Categories: _auditTrailManager.Value.DescribeCategories().ToArray(),
                Category: category);

            context.FilterDisplay.Add(dateFromFilterDisplay);
            context.FilterDisplay.Add(dateToFilterDisplay);
            context.FilterDisplay.Add(categoryFilterDisplay);
            context.FilterDisplay.Add(userNameFilterDisplay);
        }

        private DateTime? GetDateFromFilter(Filters filters, string fieldName, string prefix) {
            var dateString = filters.Get(prefix + ".Date");
            try {
                return _dateLocalizationServices.ConvertFromLocalizedDateString(dateString);
            }
            catch (FormatException ex) {
                filters.UpdateModel.AddModelError(prefix, T(@"Error parsing '{0}' date string '{1}': {2}", fieldName, dateString, ex.Message));
                return null;
            }
        }
    }
}