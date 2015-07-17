using System.Linq;
using System.Xml.Linq;
using Orchard.DynamicForms.Services;
using Orchard.Localization;
using Orchard.Recipes.Services;

namespace Orchard.DynamicForms.Recipes.Builders {
    public class FormSubmissionsStep : RecipeBuilderStep {
        private readonly IFormService _formService;
        public FormSubmissionsStep(IFormService formService) {
            _formService = formService;
        }

        public override string Name {
            get { return "FormSubmissions"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Form Submissions"); }
        }

        public override LocalizedString Description {
            get { return T("Exports submitted forms."); }
        }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            // TODO: Implement an editor that enables the user to select which forms to export.
            return null;
        }

        public override void Build(BuildContext context) {
            var submissions = _formService.GetSubmissions().ToArray();

            if (!submissions.Any()) {
                return;
            }

            var forms = submissions.GroupBy(x => x.FormName);
            var root = new XElement("Forms");
            context.RecipeDocument.Element("Orchard").Add(root);

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

