using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;

namespace Orchard.Email.Forms {
    public class EmailForm : Component, IFormProvider {
        private readonly IMessageQueueManager _messageQueueManager;
        protected dynamic New { get; set; }

        public EmailForm(IShapeFactory shapeFactory, IMessageQueueManager messageQueueManager) {
            New = shapeFactory;
            _messageQueueManager = messageQueueManager;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> formFactory =
                shape => {
                    var form = New.Form(
                        Id: "EmailActivity",
                        _Type: New.FieldSet(
                            Title: T("Send to"),
                            _RecipientAddress: New.Textbox(
                                Id: "recipient-address",
                                Name: "RecipientAddress",
                                Title: T("Email Address"),
                                Description: T("Specify a comma-separated list of recipient email addresses. To include a display name, use the following format: John Doe &lt;john.doe@outlook.com&gt;"),
                                Classes: new[] {"large", "text", "tokenized"}),
                            _Subject: New.Textbox(
                                Id: "Subject", Name: "Subject",
                                Title: T("Subject"),
                                Description: T("The subject of the email message."),
                                Classes: new[] {"large", "text", "tokenized"}),
                            _Message: New.Textarea(
                                Id: "Body", Name: "Body",
                                Title: T("Body"),
                                Description: T("The body of the email message."),
                                Classes: new[] {"tokenized"}),
                            _Priority: New.SelectList(
                                Id: "priority",
                                Name: "Priority",
                                Title: T("Priority"),
                                Description: ("The priority of this message."),
                                Items: GetPriorities())));

                    var queues = GetQueues();

                    if (queues.Count() > 1) {
                        form._Queue = New.SelectList(
                            Id: "queue",
                            Name: "Queue",
                            Title: T("Queue"),
                            Description: ("The queue to add this message to."),
                            Items: queues);
                    }

                    return form;
                };

            context.Form("EmailActivity", formFactory);
        }

        private IEnumerable<SelectListItem> GetPriorities() {
            var priorities = _messageQueueManager.GetPriorities().ToList();
            if (!priorities.Any())
                priorities = _messageQueueManager.CreateDefaultPriorities().ToList();
            return priorities.Select(x => new SelectListItem { Text = x.DisplayText, Value = x.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
        }

        private IEnumerable<SelectListItem> GetQueues() {
            var queues = _messageQueueManager.GetQueues().ToList();
            if (!queues.Any())
                queues = new List<MessageQueue> {_messageQueueManager.CreateDefaultQueue()};
            return queues.Select(x => new SelectListItem {Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture)}).ToList();
        }
    }

    public class EmailFormValidator : IFormEventHandler {
        public Localizer T { get; set; }
        public void Building(BuildingContext context) {}
        public void Built(BuildingContext context) {}
        public void Validated(ValidatingContext context) { }

        public void Validating(ValidatingContext context) {
            if (context.FormName != "EmailActivity") return;

            var recipientAddress = context.ValueProvider.GetValue("RecipientAddress").AttemptedValue;
            var subject = context.ValueProvider.GetValue("Subject").AttemptedValue;
            var body = context.ValueProvider.GetValue("Body").AttemptedValue;

            if (String.IsNullOrWhiteSpace(recipientAddress)) {
                context.ModelState.AddModelError("RecipientAddress", T("You must specify at least one recipient.").Text);
            }

            if (String.IsNullOrWhiteSpace(subject)) {
                context.ModelState.AddModelError("Subject", T("You must provide a Subject.").Text);
            }

            if (String.IsNullOrWhiteSpace(body)) {
                context.ModelState.AddModelError("Body", T("You must provide a Body.").Text);
            }
        }
    }
}