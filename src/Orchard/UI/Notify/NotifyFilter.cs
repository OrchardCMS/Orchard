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
        private const string TempDataMessages = "messages";
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
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {

            // don't touch temp data if there's no work to perform
            if (!_notifier.List().Any())
                return;

            var tempData = filterContext.Controller.TempData;

            // initialize writer with current data
            var sb = new StringBuilder();
            if (tempData.ContainsKey(TempDataMessages)) {
                sb.Append(tempData[TempDataMessages]);
            }

            // accumulate messages, one line per message
            foreach (var entry in _notifier.List()) {
                sb.Append(Convert.ToString(entry.Type))
                    .Append(':')
                    .AppendLine(entry.Message.ToString())
                    .AppendLine("-");
            }

            // assign values into temp data
            // string data type used instead of complex array to be session-friendly
            tempData[TempDataMessages] = sb.ToString();
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResult = filterContext.Result as ViewResultBase;

            // if it's not a view result, a redirect for example
            if (viewResult == null)
                return;

            var messages = Convert.ToString(viewResult.TempData[TempDataMessages]);
            if (string.IsNullOrEmpty(messages))
                return;// nothing to do, really

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

            var messagesZone = _workContextAccessor.GetContext(filterContext).Layout.Zones["Messages"];
            foreach(var messageEntry in messageEntries)
                messagesZone = messagesZone.Add(_shapeFactory.Message(messageEntry));

            //todo: (heskew) probably need to keep duplicate messages from being pushed into the zone like the previous behavior
            //baseViewModel.Messages = baseViewModel.Messages == null ? messageEntries .Messages.Union(messageEntries).ToList();
            //baseViewModel.Zones.AddRenderPartial("content:before", "Messages", baseViewModel.Messages);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}