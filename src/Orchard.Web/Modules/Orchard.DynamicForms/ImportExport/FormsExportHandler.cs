using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.DynamicForms.Services;
using Orchard.Environment.Extensions;
using Orchard.Events;

namespace Orchard.DynamicForms.ImportExport {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    [OrchardFeature("Orchard.DynamicForms.ImportExport")]
    public class FormsExportHandler : IExportEventHandler {
        private readonly IFormService _formService;
        public FormsExportHandler(IFormService formService) {
            _formService = formService;
        }

        public void Exporting(dynamic context) {
        }

        public void Exported(dynamic context) {

            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains("Forms")) {
                return;
            }

            var submissions = _formService.GetSubmissions().ToArray();

            if (!submissions.Any()) {
                return;
            }

            var forms = submissions.GroupBy(x => x.FormName);
            var root = new XElement("Forms");
            context.Document.Element("Orchard").Add(root);

            foreach (var form in forms) {
                root.Add(new XElement("Form",
                    new XAttribute("Name", form.Key),
                    new XElement("Submissions", form.Select(submission =>
                        new XElement("Submission",
                            new XAttribute("CreatedUtc", submission.CreatedUtc),
                            new XCData(submission.FormData))))));
            }
        }
    }
}

