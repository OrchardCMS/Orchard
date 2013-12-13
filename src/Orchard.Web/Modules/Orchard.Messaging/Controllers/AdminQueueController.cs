using System.Linq;
using System.Web.Mvc;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Messaging.ViewModels;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Messaging.Controllers {
    [OrchardFeature("Orchard.Messaging.Queuing")]
    [Admin]
    public class AdminQueueController : Controller {
        private readonly IMessageQueueManager _messageQueueManager;
        private readonly IOrchardServices _services;
        private readonly IMessageQueueProcessor _messageQueueProcessor;

        public AdminQueueController(IMessageQueueManager messageQueueManager, IOrchardServices services, IMessageQueueProcessor messageQueueProcessor) {
            _messageQueueManager = messageQueueManager;
            _services = services;
            _messageQueueProcessor = messageQueueProcessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var queues = _messageQueueManager.GetQueues().ToList();

            if (queues.Count == 1) {
                return RedirectToAction("List", new {id = queues.First().Id});
            }

            var queueShapes = queues.Select(x => _services.New.Queue(x)
                .Pending(_messageQueueManager.CountMessages(x.Id, QueuedMessageStatus.Pending))
                .Faulted(_messageQueueManager.CountMessages(x.Id, QueuedMessageStatus.Faulted))
                .Sent(_messageQueueManager.CountMessages(x.Id, QueuedMessageStatus.Sent))).ToList();

            var model = _services.New.ViewModel()
                .Queues(queueShapes);

            return View(model);
        }

        public ActionResult Edit(int id, string returnUrl) {
            var queue = _messageQueueManager.GetQueue(id);
            var model = new MessageQueueViewModel {
                Id = queue.Id,
                Name = queue.Name,
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(MessageQueueViewModel model) {
            if (!ModelState.IsValid)
                return View(model);

            CreateOrUpdateQueue(model);
            _services.Notifier.Information(T("Your queue has been updated."));
            return Url.IsLocalUrl(model.ReturnUrl) ? (ActionResult) Redirect(model.ReturnUrl) : RedirectToAction("Edit", new {id = model.Id});
        }

        public ActionResult Create() {
            return View(new MessageQueueViewModel());
        }

        [HttpPost]
        public ActionResult Create(MessageQueueViewModel model) {
            if (!ModelState.IsValid)
                return View(model);

            var queue = CreateOrUpdateQueue(model);
            _services.Notifier.Information(T("Your queue has been created."));
            return RedirectToAction("Edit", new { id = queue.Id });
        }

        public ActionResult List(int id, MessagesFilter filter, PagerParameters pagerParameters) {
            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var queue = _messageQueueManager.GetQueue(id);

            if (queue == null)
                return HttpNotFound();

            var messageCount = _messageQueueManager.CountMessages(queue.Id, filter.Status);
            var messages = _messageQueueManager.GetMessages(queue.Id, filter.Status, pager.GetStartIndex(), pager.PageSize).ToList();
            var model = _services.New.ViewModel()
                .Pager(_services.New.Pager(pager).TotalItemCount(messageCount))
                .Queue(queue)
                .Messages(messages)
                .Filter(filter);

            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult Filter(int id, QueuedMessageStatus? status) {
            return RedirectToAction("List", new {id, status});
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Resume")]
        public ActionResult Resume(int id, QueuedMessageStatus? status) {
            var queue = _messageQueueManager.GetQueue(id);
            _messageQueueManager.Resume(queue);
            _services.Notifier.Information(T("The queue has been resumed."));
            return RedirectToAction("List", new { id, status });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Pause")]
        public ActionResult Pause(int id, QueuedMessageStatus? status) {
            var queue = _messageQueueManager.GetQueue(id);
            _messageQueueManager.Pause(queue);
            _services.Notifier.Information(T("The queue has been paused."));
            return RedirectToAction("List", new { id, status });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Process")]
        public ActionResult Process(int id, QueuedMessageStatus? status) {
            _messageQueueProcessor.ProcessQueues();
            _services.Notifier.Information(T("Processing has started."));
            return RedirectToAction("List", new { id, status });
        }

        private MessageQueue CreateOrUpdateQueue(MessageQueueViewModel model) {
            var queue = _messageQueueManager.GetQueue(model.Id) ?? _messageQueueManager.CreateQueue();
            queue.Name = model.Name;
            return queue;
        }
    }
}