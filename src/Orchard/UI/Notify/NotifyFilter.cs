using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Filters;

namespace Orchard.UI.Notify {
    public class NotifyFilter : FilterProvider, IActionFilter, IResultFilter {
        public const string TempDataMessages = "Messages";
        private readonly INotifier _notifier;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public NotifyFilter(
            INotifier notifier, 
            IWorkContextAccessor workContextAccessor, 
            IShapeFactory shapeFactory) {
            _notifier = notifier;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            var messages = Convert.ToString(filterContext.Controller.TempData[TempDataMessages]);
            if (String.IsNullOrEmpty(messages))
                return;

            var messageEntries = new List<NotifyEntry>();
            foreach (var line in messages.Split(new[] { System.Environment.NewLine + "-" + System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                var delimiterIndex = line.IndexOf(':');
                if (delimiterIndex != -1) {
                    var type = (NotifyType)Enum.Parse(typeof(NotifyType), line.Substring(0, delimiterIndex));
                    var message = new LocalizedString(line.Substring(delimiterIndex + 1));
                    if (!messageEntries.Any(ne => ne.Message.TextHint == message.TextHint)) {
                        messageEntries.Add(new NotifyEntry {
                            Type = type,
                            Message = message
                        });
                    }
                }
                else {
                    var message = new LocalizedString(line.Substring(delimiterIndex + 1));
                    if (!messageEntries.Any(ne => ne.Message.TextHint == message.TextHint)) {
                        messageEntries.Add(new NotifyEntry {
                            Type = NotifyType.Information,
                            Message = message
                        });
                    }
                }
            }

            if (!messageEntries.Any())
                return;

            // Make the notifications available for the rest of the current request.
            filterContext.HttpContext.Items[TempDataMessages] = messageEntries;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {

            // Don't touch temp data if there's no work to perform.
            if (!_notifier.List().Any())
                return;

            var messageEntries = _notifier.List().ToList();

            if (filterContext.Result is ViewResultBase) {
                // Assign values to the Items collection instead of TempData and 
                // combine any existing entries added by the previous request with new ones.
                var existingEntries = filterContext.HttpContext.Items[TempDataMessages] as IList<NotifyEntry> ?? new List<NotifyEntry>();
                messageEntries = messageEntries.Concat(existingEntries).ToList();
                filterContext.HttpContext.Items[TempDataMessages] = messageEntries;

                return;
            }
            
            var tempData = filterContext.Controller.TempData;

            // Initialize writer with current data.
            var sb = new StringBuilder();
            if (tempData.ContainsKey(TempDataMessages)) {
                sb.Append(tempData[TempDataMessages]);
            }

            // Accumulate messages, one line per message.
            foreach (var entry in messageEntries) {
                sb.Append(Convert.ToString(entry.Type))
                    .Append(':')
                    .AppendLine(entry.Message.ToString())
                    .AppendLine("-");
            }

            // Result is not a view, so assume a redirect and assign values to TemData.
            // String data type used instead of complex array to be session-friendly.
            tempData[TempDataMessages] = sb.ToString();
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            if (!(filterContext.Result is ViewResultBase))
                return;

            var messageEntries = filterContext.HttpContext.Items[TempDataMessages] as IList<NotifyEntry> ?? new List<NotifyEntry>();
            var messagesZone = _workContextAccessor.GetContext(filterContext).Layout.Zones["Messages"];
            foreach (var messageEntry in messageEntries)
                messagesZone = messagesZone.Add(_shapeFactory.Message(messageEntry));

            //todo: (heskew) probably need to keep duplicate messages from being pushed into the zone like the previous behavior
            //baseViewModel.Messages = baseViewModel.Messages == null ? messageEntries .Messages.Union(messageEntries).ToList();
            //baseViewModel.Zones.AddRenderPartial("content:before", "Messages", baseViewModel.Messages);
        }
        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}