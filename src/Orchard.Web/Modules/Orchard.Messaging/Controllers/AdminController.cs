using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Messaging.ViewModels;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Messaging.Controllers {
    [Admin]
    public class AdminController : Controller {
        private readonly IMessageQueueService _messageQueueManager;
        private readonly IOrchardServices _services;
        private readonly IMessageQueueProcessor _messageQueueProcessor;

        public AdminController(
            IMessageQueueService messageQueueManager, 
            IShapeFactory shapeFactory,
            IOrchardServices services, 
            IMessageQueueProcessor messageQueueProcessor) {
            _messageQueueManager = messageQueueManager;
            _services = services;
            _messageQueueProcessor = messageQueueProcessor;
            New = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public dynamic New { get; set; }
        public Localizer T { get; set; }
        public ActionResult Details(int id, string returnUrl) {
            var message = _messageQueueManager.GetMessage(id);

            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.Action("List");

            var model = New.ViewModel().Message(message).ReturnUrl(returnUrl);
            return View(model);
        }

        public ActionResult List(MessagesFilter filter, PagerParameters pagerParameters) {
            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);

            var messageCount = _messageQueueManager.GetMessagesCount(filter.Status);
            var messages = _messageQueueManager.GetMessages(filter.Status, pager.GetStartIndex(), pager.PageSize).ToList();
            var model = _services.New.ViewModel()
                .Pager(_services.New.Pager(pager).TotalItemCount(messageCount))
                .MessageQueueStatus(_services.WorkContext.CurrentSite.As<MessageSettingsPart>().Status)
                .Messages(messages)
                .Filter(filter);

            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult Filter(QueuedMessageStatus? status) {
            return RedirectToAction("List", new { status });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Resume")]
        public ActionResult Resume(QueuedMessageStatus? status) {
            _messageQueueManager.Resume();
            _services.Notifier.Information(T("The queue has been resumed."));
            return RedirectToAction("List", new { status });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Pause")]
        public ActionResult Pause(QueuedMessageStatus? status) {
            _messageQueueManager.Pause();
            _services.Notifier.Information(T("The queue has been paused."));
            return RedirectToAction("List", new { status });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Process")]
        public ActionResult Process(QueuedMessageStatus? status) {
            _messageQueueProcessor.ProcessQueue();
            _services.Notifier.Information(T("Processing has started."));
            return RedirectToAction("List", new { status });
        }

    }
}