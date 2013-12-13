using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Messaging.Extensions;
using Orchard.Messaging.Services;
using Orchard.Messaging.ViewModels;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Messaging.Controllers {
    [OrchardFeature("Orchard.Messaging.Queuing")]
    [Admin]
    public class AdminPriorityController : Controller {
        private readonly IMessageQueueManager _messageQueueManager;
        private readonly INotifier _notifier;

        public AdminPriorityController(IMessageQueueManager messageQueueManager, IShapeFactory shapeFactory, INotifier notifier) {
            _messageQueueManager = messageQueueManager;
            _notifier = notifier;
            New = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public dynamic New { get; set; }

        public ActionResult Index() {
            var priorities = _messageQueueManager.GetPriorities().ToList();
            var model = New.ViewModel().Priorities(priorities);
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            var priority = _messageQueueManager.GetPriority(id);
            _messageQueueManager.DeletePriority(priority);
            _notifier.Information(T("That priority has been deleted."));
            return RedirectToAction("Index");
        }

        public ActionResult Create() {
            return View(new MessagePriorityViewModel());
        }

        [HttpPost]
        public ActionResult Create(MessagePriorityViewModel model) {
            if (!ModelState.IsValid)
                return View(model);

            var priority = _messageQueueManager.CreatePriority(model.Name.TrimSafe(), model.DisplayText.TrimSafe(), model.Value);
            _notifier.Information(T("Your Priority has been created."));
            return RedirectToAction("Edit", new { priority.Id });
        }

        public ActionResult Edit(int id) {
            var priority = _messageQueueManager.GetPriority(id);
            if (priority == null || priority.Archived)
                return HttpNotFound();
            
            var model = new MessagePriorityViewModel {
                Id = priority.Id,
                Name = priority.Name,
                DisplayText = priority.DisplayText,
                Value = priority.Value
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(MessagePriorityViewModel model) {
            if (!ModelState.IsValid)
                return View(model);

            var priority = _messageQueueManager.GetPriority(model.Id);
            priority.Name = model.Name.TrimSafe();
            priority.DisplayText = model.DisplayText.TrimSafe();
            priority.Value = model.Value;

            _notifier.Information(T("Your Priority has been updated."));
            return RedirectToAction("Edit", new { priority.Id });
        }
    }
}