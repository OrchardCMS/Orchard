using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;

namespace Orchard.UI.Notify {
    public class NotifyFilter : FilterProvider, IActionFilter, IResultFilter {
        private const string TempDataMessages = "messages";
        private readonly INotifier _notifier;

        public NotifyFilter(INotifier notifier) {
            _notifier = notifier;
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
                    .AppendLine(entry.Message);
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

            var baseViewModel = viewResult.ViewData.Model as BaseViewModel;
            // if it's not a view model that holds messages, don't touch temp data either
            if (baseViewModel == null)
                return;

            var messages = Convert.ToString(viewResult.TempData[TempDataMessages]);
            if (string.IsNullOrEmpty(messages))
                return;// nothing to do, really

            var messageEntries = new List<NotifyEntry>();
            foreach (var line in messages.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                var delimiterIndex = line.IndexOf(':');
                if (delimiterIndex != -1) {
                    var type = (NotifyType)Enum.Parse(typeof(NotifyType), line.Substring(0, delimiterIndex));
                    messageEntries.Add(new NotifyEntry {
                                                           Type = type,
                                                           Message = line.Substring(delimiterIndex + 1)
                                                       });
                }
                else {
                    messageEntries.Add(new NotifyEntry {
                                                           Type = NotifyType.Information,
                                                           Message = line.Substring(delimiterIndex + 1)
                                                       });
                }
            }

            baseViewModel.Messages = baseViewModel.Messages == null ? messageEntries : baseViewModel.Messages.Union(messageEntries).ToList();
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}