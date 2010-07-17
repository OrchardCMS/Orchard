using System.Linq;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;

namespace Orchard.UI.Admin.Notification {
    public class AdminNotificationFilter : FilterProvider, IResultFilter {
        private readonly INotificationManager _notificationManager;

        public AdminNotificationFilter(INotificationManager notificationManager) {
            _notificationManager = notificationManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {

            if ( !AdminFilter.IsApplied(filterContext.RequestContext) ) {
                return;
            }
            var viewResult = filterContext.Result as ViewResultBase;

            // if it's not a view result, a redirect for example
            if ( viewResult == null )
                return;

            var baseViewModel = BaseViewModel.From(viewResult);
            // if it's not a view model that holds messages, don't touch temp data either
            if ( baseViewModel == null )
                return;
            
            var messageEntries = _notificationManager.GetNotifications().ToList();

            if ( messageEntries.Any() ) {
                baseViewModel.Zones.AddRenderPartial("content:before", "Messages", messageEntries);
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}
