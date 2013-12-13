using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Messaging.Services;
using Orchard.UI.Admin;

namespace Orchard.Messaging.Controllers {
    [OrchardFeature("Orchard.Messaging.Queuing")]
    [Admin]
    public class AdminMessageController : Controller {
        private readonly IMessageQueueManager _messageQueueManager;
        
        public AdminMessageController(IMessageQueueManager messageQueueManager, IShapeFactory shapeFactory) {
            _messageQueueManager = messageQueueManager;
            New = shapeFactory;
        }

        public dynamic New { get; set; }

        public ActionResult Details(int id, string returnUrl) {
            var message = _messageQueueManager.GetMessage(id);

            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.Action("List", "AdminQueue", new {message.Queue.Id});

            var model = New.ViewModel().Message(message).ReturnUrl(returnUrl);
            return View(model);
        }

    }
}