using System;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.Scripting.Rules {
    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }

    [OrchardFeature("Orchard.Scripting.Rules")]
    public class ConditionForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ConditionForms(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context) {
            Func<IShapeFactory, dynamic> form =
                shape => Shape.Form(
                Id: "ScriptCondition",
                _Description: Shape.Textbox(
                    Id: "description", Name: "description",
                    Title: T("Description"),
                    Description: T("Message that will be displayed in the Actions list."),
                    Classes: new[] { "textMedium" }),
                _Condition: Shape.TextArea(
                    Id: "condition", Name: "condition",
                    Title: T("Condition"),
                    Description: T("Enter a valid boolean expression to evaluate."),
                    Classes: new[] { "tokenized" })
                );

            context.Form("ScriptCondition", form);
        }
    }
}