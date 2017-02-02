using System.Linq;
using System.Xml.Linq;
using Orchard.DynamicForms.Services;
using Orchard.Localization;
using Orchard.Recipes.Services;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.DynamicForms.ViewModels;

namespace Orchard.DynamicForms.Recipes.Builders {
    public class FormSubmissionsStep : RecipeBuilderStep {
        private readonly IFormService _formService;

        public FormSubmissionsStep(IFormService formService) {
            _formService = formService;
            SelectedForms = new List<string>();
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

        public IList<string> SelectedForms { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            List<FormExportEntry> forms = new List<FormExportEntry>();

            if (updater != null && updater.TryUpdateModel(forms, Prefix, null, null)) {
                SelectedForms = forms.Where(x => x.Export).Select(x => x.FormName).ToList();
            }
            else {
                forms = _formService.GetSubmissions().OrderBy(x => x.FormName).GroupBy(y => y.FormName)
                    .Select(x => new FormExportEntry { FormName = x.FirstOrDefault().FormName })
                    .ToList();
            }

            return shapeFactory.EditorTemplate(TemplateName: "BuilderSteps/FormSubmissions", Model: forms, Prefix: Prefix);
        }

        public override void Build(BuildContext context) {
            if (SelectedForms.Count() > 0) {
                var root = new XElement("Forms");
                context.RecipeDocument.Element("Orchard").Add(root);

                foreach (var form in SelectedForms) {
                    var submissions = _formService.GetSubmissions(form);
                    if (submissions.Count() > 0) {
                        root.Add(new XElement("Form",
                            new XAttribute("Name", form),
                            new XElement("Submissions", submissions.Select(submission =>
                                new XElement("Submission",
                                    new XAttribute("CreatedUtc", submission.CreatedUtc),
                                    new XCData(submission.FormData))))));
                    }
                }
            }
        }
    }
}

