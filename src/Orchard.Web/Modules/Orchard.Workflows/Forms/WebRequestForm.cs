using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Workflows.Forms {
    public class WebRequestForm : IFormProvider {
        protected dynamic New { get; set; }
        public Localizer T { get; set; }

        public WebRequestForm(IShapeFactory shapeFactory) {
            New = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            context.Form("WebRequestActivity",
                shape => {
                    var form = New.Form(
                        Id: "WebRequestActivity",
                        _Url: New.Textbox(
                            Id: "Url", Name: "Url",
                            Title: T("Url"),
                            Description: T("The url to send the request to."),
                            Classes: new[] {"text large", "tokenized"}),
                        _Verb: New.SelectList(
                            Id: "Verb", Name: "Verb",
                            Title: T("Verb"),
                            Description: T("The HTTP verb to use.")),
                        _Headers: New.Textarea(
                            Id: "Headers", Name: "Headers",
                            Title: T("Headers"),
                            Description: T("Enter one line per header=value pair"),
                            Classes: new[] {"tokenized"}),
                        _FormFormat: New.SelectList(
                            Id: "FormFormat", Name: "FormFormat",
                            Title: T("Form Format"),
                            Description: T("The serialization format to use for the POST body.")),
                        _FormValues: New.Textarea(
                            Id: "FormValues", Name: "FormValues",
                            Title: T("Form Values"),
                            Description: T("For KeyValue, enter one line per key=value pair to submit when using the POST verb. For JSON, enter a string where the curly braces are replaced by double brackets e.g. { test: 'test' } becomes (( test: 'test' ))."),
                            Classes: new[] {"tokenized"})
                        );

                    form._Verb.Add(new SelectListItem { Value = "GET", Text = "GET" });
                    form._Verb.Add(new SelectListItem { Value = "POST", Text = "POST" });
                    form._Verb.Add(new SelectListItem { Value = "PUT", Text = "PUT" });
                    form._Verb.Add(new SelectListItem { Value = "DELETE", Text = "DELETE" });

                    form._FormFormat.Add(new SelectListItem { Value = "KeyValue", Text = "Key / Value" });
                    form._FormFormat.Add(new SelectListItem { Value = "Json", Text = "Json" });

                    return form;
                }
            );
        }
    }
}
