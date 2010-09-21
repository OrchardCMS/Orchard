using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;

namespace Orchard.UI.Admin.Notification {
    public class AdminNotificationFilter : FilterProvider, IResultFilter {
        private readonly INotificationManager _notificationManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IShapeHelperFactory _shapeHelperFactory;

        public AdminNotificationFilter(INotificationManager notificationManager, IWorkContextAccessor workContextAccessor, IShapeHelperFactory shapeHelperFactory) {
            _notificationManager = notificationManager;
            _workContextAccessor = workContextAccessor;
            _shapeHelperFactory = shapeHelperFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            if (!AdminFilter.IsApplied(filterContext.RequestContext))
                return;

            // if it's not a view result, a redirect for example
            if (!(filterContext.Result is ViewResultBase))
                return;
            
            var messageEntries = _notificationManager.GetNotifications().ToList();
            if (!messageEntries.Any())
                return;

            var shape = _shapeHelperFactory.CreateHelper();
            var messagesZone = _workContextAccessor.GetContext(filterContext).Page.Zones["Messages"];
            foreach(var messageEntry in messageEntries)
                messagesZone.Add(shape.Message(messageEntry));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}
