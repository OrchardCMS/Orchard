using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Newtonsoft.Json;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Workflows.Activities;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.Forms {
    public class SignalForms : IFormProvider {
        private readonly IRepository<ActivityRecord> _activityRecords;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public SignalForms(IShapeFactory shapeFactory, IRepository<ActivityRecord> activityRecords) {
            _activityRecords = activityRecords;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    return Shape.Form(
                        Id: "SignalEvent",
                        _Name: Shape.Textbox(
                            Id: "signal", Name: "Signal",
                            Title: T("Name of the signal."),
                            Description: T("The name of the signal."),
                            Classes: new[] { "text medium" })
                        );
                };

            context.Form("SignalEvent", form);

            form =
                shape => {
                    var f = Shape.Form(
                        Id: "OneOfSignals",
                        _Parts: Shape.SelectList(
                            Id: "signal", Name: "Signal",
                            Title: T("Available signals"),
                            Description: T("Select a signal."),
                            Size: 1,
                            Multiple: false
                            )
                        );

                    var allEvents = _activityRecords
                        .Table
                        .Where(x => x.Name == SignalActivity.SignalEventName)
                        .Select(x => GetState(x.State))
                        .ToArray()
                        .Select(x => (string)x.Signal);

                    foreach (var signal in allEvents) {
                        f._Parts.Add(new SelectListItem { Value = signal, Text = signal });
                    }

                    return f;
                };

            context.Form("Trigger", form);
        }

        private dynamic GetState(string state) {
            if (!String.IsNullOrWhiteSpace(state)) {
                var formatted = JsonConvert.DeserializeXNode(state, "Root").ToString();
                var serialized = String.IsNullOrEmpty(formatted) ? "{}" : JsonConvert.SerializeXNode(XElement.Parse(formatted));
                return FormParametersHelper.FromJsonString(serialized).Root;
            }

            return FormParametersHelper.FromJsonString("{}");
        }
    }
}