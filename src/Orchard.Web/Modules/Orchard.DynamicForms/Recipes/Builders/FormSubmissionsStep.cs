using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.DynamicForms.Recipes.Builders {
    public class FormSubmissionsStep : RecipeBuilderStep {
        private readonly IFormService _formService;
        public FormSubmissionsStep(IFormService formService) {
            _formService = formService;
            SelectedSubmissions = new List<string>();
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
        public IList<string> SelectedSubmissions { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            // TODO: Implement an editor that enables the user to select which forms to export.
            return UpdateEditor(shapeFactory, null);
        }
        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var submissions = _formService.GetSubmissions().OrderBy(x => x.FormName).GroupBy(y => y.FormName)
                .Select(x => new FormSubmissionsEntry { Name = x.FirstOrDefault().FormName })
                .ToList(); 

            if (!submissions.Any()) {
                return null;
            }
         
            var viewModel = new FormSubmissionsBuilderStepViewModel {
                Submissions = submissions
            };

            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                SelectedSubmissions = viewModel.Submissions.Where(x => x.ExportFormData).Select(x => x.Name).ToList();
            }

            return shapeFactory.EditorTemplate(TemplateName: "BuilderSteps/Submissions", Model: viewModel, Prefix: Prefix);
        }
        public override void Configure(RecipeBuilderStepConfigurationContext context) {
        
            var selectedSubmissions = context.ConfigurationElement.Attr("SelectedSubmissions");
        
            if (!String.IsNullOrWhiteSpace(selectedSubmissions))
                SelectedSubmissions = selectedSubmissions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();


        }

        public override void ConfigureDefault() {

            SelectedSubmissions = _formService.GetSubmissions().OrderBy(x => x.FormName).GroupBy(y => y.FormName)
               .Select(x => x.FirstOrDefault().FormName).ToList();
                
        }
        public override void Build(BuildContext context) {
            var submissions = _formService.GetSubmissions();
            var selectedSubmissions = SelectedSubmissions;
            var forms = submissions.Where(a => SelectedSubmissions.Contains(a.FormName)).ToArray().GroupBy(x => x.FormName);
            if (!forms.Any()) {
                return;
            }
            
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

